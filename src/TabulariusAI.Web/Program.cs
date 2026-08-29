using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Data.Identity;
using TabulariusAI.Web.Middleware;
using TabulariusAI.Web.Models;
using TabulariusAI.Web.Services;

var builder = WebApplication.CreateBuilder(args);
var dataDirectory = Path.GetFullPath(builder.Configuration["Storage:DataDirectory"] ?? Path.Combine(builder.Environment.ContentRootPath, "data"));
Directory.CreateDirectory(dataDirectory);
var logDirectory = Path.Combine(builder.Environment.ContentRootPath, "logs");
Directory.CreateDirectory(logDirectory);

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().Enrich.WithProperty("Application", "TabulariusAI").WriteTo.Console().WriteTo.File(Path.Combine(logDirectory, "tabularius-.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}").CreateLogger();
builder.Host.UseSerilog();
builder.Services.AddControllersWithViews();

var databaseProvider = builder.Configuration["Database:Provider"]?.Trim().ToLowerInvariant() ?? "sqlite";
builder.Services.AddDbContext<TabulariusDbContext>(options =>
{
    if (databaseProvider == "sqlite")
    {
        var configuredConnection = builder.Configuration.GetConnectionString("Tabularius");
        var connectionString = string.IsNullOrWhiteSpace(configuredConnection) || configuredConnection.Contains("MSSQLLocalDB", StringComparison.OrdinalIgnoreCase) ? $"Data Source={Path.Combine(dataDirectory, "tabularius.db")}" : configuredConnection;
        options.UseSqlite(connectionString);
    }
    else if (databaseProvider == "sqlserver") options.UseSqlServer(builder.Configuration.GetConnectionString("Tabularius") ?? throw new InvalidOperationException("ConnectionStrings:Tabularius is required for SQL Server mode."));
    else throw new InvalidOperationException($"Unsupported database provider '{databaseProvider}'. Use 'Sqlite' or 'SqlServer'.");
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => { options.Password.RequiredLength = 12; options.Password.RequireDigit = true; options.Password.RequireLowercase = true; options.Password.RequireUppercase = true; options.Password.RequireNonAlphanumeric = true; options.User.RequireUniqueEmail = true; options.SignIn.RequireConfirmedAccount = false; options.Lockout.MaxFailedAccessAttempts = 5; options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); }).AddEntityFrameworkStores<TabulariusDbContext>().AddDefaultTokenProviders();
var googleClientId = builder.Configuration["Authentication:Google:ClientId"]; var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret)) builder.Services.AddAuthentication().AddGoogle(options => { options.ClientId = googleClientId; options.ClientSecret = googleClientSecret; options.SaveTokens = false; });
builder.Services.ConfigureApplicationCookie(options => { options.Cookie.Name = "TabulariusAI.Auth"; options.Cookie.HttpOnly = true; options.Cookie.SameSite = SameSiteMode.Lax; options.LoginPath = "/Account/Login"; options.AccessDeniedPath = "/Account/AccessDenied"; options.SlidingExpiration = true; options.ExpireTimeSpan = TimeSpan.FromHours(8); });
builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()); builder.Services.AddScoped<ISaftHeaderReader, SaftHeaderReader>();
var applicationAssembly = typeof(Program).Assembly; var applicationVersion = applicationAssembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false).OfType<System.Reflection.AssemblyInformationalVersionAttribute>().SingleOrDefault()?.InformationalVersion.Split('+')[0] ?? applicationAssembly.GetName().Version?.ToString(3) ?? "0.1.0"; builder.Services.AddSingleton(new ApplicationInfo(applicationVersion, "Análise e controlo contabilístico"));
var app = builder.Build(); await InitializeDatabaseAsync(app, databaseProvider); await SeedIdentityAsync(app);
app.UseSerilogRequestLogging(options => { options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms"; }); if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); } app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.UseAuthentication(); app.UseMiddleware<MandatoryPasswordChangeMiddleware>(); app.UseAuthorization(); app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
try { Log.Information("Starting Tabularius AI version {Version} in {DatabaseProvider} database mode", applicationVersion, databaseProvider); app.Run(); } catch (Exception exception) { Log.Fatal(exception, "Tabularius AI terminated unexpectedly"); throw; } finally { Log.CloseAndFlush(); }

/// <summary>Initializes the configured database provider for the current deployment mode.</summary>
/// <param name="application">The running web application.</param>
/// <param name="provider">The normalized database provider name.</param>
/// <returns>A task representing the asynchronous database initialization operation.</returns>
static async Task InitializeDatabaseAsync(WebApplication application, string provider)
{
    await using var scope = application.Services.CreateAsyncScope(); var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitialization");
    try { var dbContext = scope.ServiceProvider.GetRequiredService<TabulariusDbContext>(); if (provider == "sqlite") await dbContext.Database.EnsureCreatedAsync(); else await dbContext.Database.MigrateAsync(); logger.LogInformation("Database initialized successfully using {Provider}.", provider); }
    catch (Exception exception) { logger.LogCritical(exception, "Database initialization failed using {Provider}.", provider); throw; }
}

/// <summary>Ensures the application roles and the local bootstrap administrator exist and repairs bootstrap duplicates left by interrupted initialization attempts.</summary>
/// <param name="application">The running web application.</param><returns>A task representing the asynchronous identity seed operation.</returns>
static async Task SeedIdentityAsync(WebApplication application)
{
    const string administratorName = "admin"; const string administratorEmail = "admin@tabularius.local"; const string temporaryPassword = "LetMeIn"; await using var scope = application.Services.CreateAsyncScope(); var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var roleName in ApplicationRoles.All) if (!await roleManager.RoleExistsAsync(roleName)) { var roleCreateResult = await roleManager.CreateAsync(new IdentityRole(roleName)); if (!roleCreateResult.Succeeded) throw new InvalidOperationException($"Application role could not be created: {string.Join("; ", roleCreateResult.Errors.Select(error => error.Code))}"); }
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(); var dbContext = scope.ServiceProvider.GetRequiredService<TabulariusDbContext>(); var normalizedAdministratorName = userManager.NormalizeName(administratorName); var normalizedAdministratorEmail = userManager.NormalizeEmail(administratorEmail); var bootstrapUsers = await dbContext.Users.Where(user => user.UserName == administratorName || user.NormalizedUserName == normalizedAdministratorName).OrderBy(user => user.CreatedAtUtc).ThenBy(user => user.Id).ToListAsync(); ApplicationUser administrator;
    if (bootstrapUsers.Count == 0) { administrator = new ApplicationUser { UserName = administratorName, NormalizedUserName = normalizedAdministratorName, Email = administratorEmail, NormalizedEmail = normalizedAdministratorEmail, EmailConfirmed = true, DisplayName = "Administrador" }; var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>(); administrator.PasswordHash = passwordHasher.HashPassword(administrator, temporaryPassword); administrator.SecurityStamp = Guid.NewGuid().ToString(); dbContext.Users.Add(administrator); await dbContext.SaveChangesAsync(); }
    else { administrator = bootstrapUsers[0]; var duplicateUsers = bootstrapUsers.Skip(1).ToList(); if (duplicateUsers.Count > 0) { var duplicateIds = duplicateUsers.Select(user => user.Id).ToArray(); var duplicateRoles = await dbContext.UserRoles.Where(item => duplicateIds.Contains(item.UserId)).ToListAsync(); if (duplicateRoles.Count > 0) dbContext.UserRoles.RemoveRange(duplicateRoles); dbContext.Users.RemoveRange(duplicateUsers); } administrator.UserName = administratorName; administrator.NormalizedUserName = normalizedAdministratorName; administrator.Email = administratorEmail; administrator.NormalizedEmail = normalizedAdministratorEmail; administrator.EmailConfirmed = true; if (string.IsNullOrWhiteSpace(administrator.DisplayName)) administrator.DisplayName = "Administrador"; await dbContext.SaveChangesAsync(); }
    var administratorRole = await roleManager.FindByNameAsync(ApplicationRoles.Administrator) ?? throw new InvalidOperationException("Administrator role was not found after initialization."); var roleAlreadyAssigned = await dbContext.UserRoles.AnyAsync(item => item.UserId == administrator.Id && item.RoleId == administratorRole.Id); if (!roleAlreadyAssigned) { dbContext.UserRoles.Add(new IdentityUserRole<string> { UserId = administrator.Id, RoleId = administratorRole.Id }); await dbContext.SaveChangesAsync(); }
}

public partial class Program;
