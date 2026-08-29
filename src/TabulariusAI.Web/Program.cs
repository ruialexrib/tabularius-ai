using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Data.Identity;
using TabulariusAI.Web.Models;
using TabulariusAI.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var logDirectory = Path.Combine(builder.Environment.ContentRootPath, "logs");
Directory.CreateDirectory(logDirectory);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "TabulariusAI")
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(logDirectory, "tabularius-.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<TabulariusDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Tabularius")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
}).AddEntityFrameworkStores<TabulariusDbContext>().AddDefaultTokenProviders();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    builder.Services.AddAuthentication().AddGoogle(options => { options.ClientId = googleClientId; options.ClientSecret = googleClientSecret; options.SaveTokens = false; });
}

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "TabulariusAI.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});
builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddScoped<ISaftHeaderReader, SaftHeaderReader>();
var applicationAssembly = typeof(Program).Assembly;
var applicationVersion = applicationAssembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false).OfType<System.Reflection.AssemblyInformationalVersionAttribute>().SingleOrDefault()?.InformationalVersion.Split('+')[0] ?? applicationAssembly.GetName().Version?.ToString(3) ?? "0.1.0";
builder.Services.AddSingleton(new ApplicationInfo(applicationVersion, "Análise e controlo contabilístico"));

var app = builder.Build();
await ApplyDatabaseMigrationsAsync(app);
await SeedIdentityAsync(app);

app.UseSerilogRequestLogging(options => { options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms"; });
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

try { Log.Information("Starting Tabularius AI version {Version}", applicationVersion); app.Run(); }
catch (Exception exception) { Log.Fatal(exception, "Tabularius AI terminated unexpectedly"); throw; }
finally { Log.CloseAndFlush(); }

/// <summary>Applies pending Entity Framework Core migrations to the local Tabularius database.</summary>
/// <param name="application">The running web application.</param>
/// <returns>A task representing the asynchronous migration operation.</returns>
static async Task ApplyDatabaseMigrationsAsync(WebApplication application)
{
    await using var scope = application.Services.CreateAsyncScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigration");
    try { var dbContext = scope.ServiceProvider.GetRequiredService<TabulariusDbContext>(); await dbContext.Database.MigrateAsync(); logger.LogInformation("Local database migrations applied successfully."); }
    catch (Exception exception) { logger.LogCritical(exception, "Local database migration failed during application startup."); throw; }
}

/// <summary>Ensures the application roles and the local bootstrap administrator exist.</summary>
/// <param name="application">The running web application.</param>
/// <returns>A task representing the asynchronous identity seed operation.</returns>
static async Task SeedIdentityAsync(WebApplication application)
{
    const string administratorName = "admin";
    const string temporaryPassword = "LetMeIn";

    await using var scope = application.Services.CreateAsyncScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in ApplicationRoles.All)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var roleCreateResult = await roleManager.CreateAsync(new IdentityRole(role));
            if (!roleCreateResult.Succeeded) throw new InvalidOperationException($"Application role could not be created: {string.Join("; ", roleCreateResult.Errors.Select(error => error.Code))}");
        }
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var administrator = await userManager.FindByNameAsync(administratorName);
    if (administrator is null)
    {
        administrator = new ApplicationUser { UserName = administratorName, DisplayName = "Administrador" };
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();
        administrator.PasswordHash = passwordHasher.HashPassword(administrator, temporaryPassword);
        administrator.SecurityStamp = Guid.NewGuid().ToString();

        var userStore = scope.ServiceProvider.GetRequiredService<IUserStore<ApplicationUser>>();
        var createResult = await userStore.CreateAsync(administrator, CancellationToken.None);
        if (!createResult.Succeeded) throw new InvalidOperationException($"Bootstrap administrator could not be created: {string.Join("; ", createResult.Errors.Select(error => error.Code))}");
    }

    var role = await roleManager.FindByNameAsync(ApplicationRoles.Administrator) ?? throw new InvalidOperationException("Administrator role was not found after initialization.");
    var dbContext = scope.ServiceProvider.GetRequiredService<TabulariusDbContext>();
    var roleAlreadyAssigned = await dbContext.UserRoles.AnyAsync(item => item.UserId == administrator.Id && item.RoleId == role.Id);
    if (!roleAlreadyAssigned)
    {
        dbContext.UserRoles.Add(new IdentityUserRole<string> { UserId = administrator.Id, RoleId = role.Id });
        await dbContext.SaveChangesAsync();
    }
}

public partial class Program;
