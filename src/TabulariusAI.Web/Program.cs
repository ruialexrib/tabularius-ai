using Microsoft.EntityFrameworkCore;
using Serilog;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var logDirectory = Path.Combine(builder.Environment.ContentRootPath, "logs");
Directory.CreateDirectory(logDirectory);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "TabulariusAI")
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(logDirectory, "tabularius-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<TabulariusDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Tabularius")));
builder.Services.AddScoped<ISaftHeaderReader, SaftHeaderReader>();

var app = builder.Build();

await ApplyDatabaseMigrationsAsync(app);

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

try
{
    Log.Information("Starting Tabularius AI version {Version}", typeof(Program).Assembly.GetName().Version?.ToString());
    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Tabularius AI terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Applies pending Entity Framework Core migrations to the local Tabularius database.
/// </summary>
/// <param name="application">The running web application.</param>
/// <returns>A task representing the asynchronous migration operation.</returns>
static async Task ApplyDatabaseMigrationsAsync(WebApplication application)
{
    await using var scope = application.Services.CreateAsyncScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigration");

    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TabulariusDbContext>();
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Local database migrations applied successfully.");
    }
    catch (Exception exception)
    {
        logger.LogCritical(exception, "Local database migration failed during application startup.");
        throw;
    }
}
