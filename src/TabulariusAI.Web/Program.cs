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
