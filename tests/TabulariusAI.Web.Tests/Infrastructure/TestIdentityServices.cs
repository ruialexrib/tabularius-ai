using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Data.Identity;
using Xunit;

namespace TabulariusAI.Web.Tests.Infrastructure;

/// <summary>Provides real ASP.NET Core Identity services backed by the isolated relational test database.</summary>
public sealed class TestIdentityServices : IAsyncDisposable
{
    private readonly ServiceProvider provider;
    private readonly TabulariusDbContext context;

    /// <summary>Initializes Identity with the same relevant password and lockout rules used by the application.</summary>
    public TestIdentityServices(TabulariusDbContext context)
    {
        this.context = context;
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvcCore();
        services.AddSingleton(context);
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 12;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<TabulariusDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();
        provider = services.BuildServiceProvider();
        HttpContext = new DefaultHttpContext { RequestServices = provider };
        provider.GetRequiredService<IHttpContextAccessor>().HttpContext = HttpContext;
    }

    /// <summary>Gets the HTTP context used by Identity sign-in operations.</summary>
    public DefaultHttpContext HttpContext { get; }

    /// <summary>Gets the configured application user manager.</summary>
    public UserManager<ApplicationUser> UserManager => provider.GetRequiredService<UserManager<ApplicationUser>>();

    /// <summary>Gets the configured application sign-in manager.</summary>
    public SignInManager<ApplicationUser> SignInManager => provider.GetRequiredService<SignInManager<ApplicationUser>>();

    /// <summary>Ensures an application role exists in the relational test database.</summary>
    public Task EnsureRoleExistsAsync(string role) => EnsureRoleAsync(role);

    /// <summary>Creates a valid application user with the supplied credentials, including the bootstrap temporary credential used by production initialization.</summary>
    public async Task<ApplicationUser> CreateUserAsync(string userName, string password, string role = ApplicationRoles.User)
    {
        await EnsureRoleAsync(role);
        var user = new ApplicationUser { UserName = userName, Email = $"{userName}@tabularius.local", EmailConfirmed = true, DisplayName = userName, LockoutEnabled = true };
        var creationPassword = password == "LetMeIn" ? "Bootstrap123!" : password;
        Assert.True((await UserManager.CreateAsync(user, creationPassword)).Succeeded);
        if (password == "LetMeIn")
        {
            user.PasswordHash = UserManager.PasswordHasher.HashPassword(user, password);
            user.SecurityStamp = Guid.NewGuid().ToString();
            await context.SaveChangesAsync();
        }
        Assert.True((await UserManager.AddToRoleAsync(user, role)).Succeeded);
        return user;
    }

    /// <summary>Ensures the supplied application role exists in the relational test database.</summary>
    private async Task EnsureRoleAsync(string role)
    {
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        if (await roleManager.RoleExistsAsync(role)) return;
        var identityRole = new IdentityRole(role)
        {
            NormalizedName = roleManager.NormalizeKey(role)
        };
        context.Roles.Add(identityRole);
        await context.SaveChangesAsync();
    }

    /// <summary>Disposes the Identity service provider.</summary>
    public ValueTask DisposeAsync()
    {
        provider.Dispose();
        return ValueTask.CompletedTask;
    }
}
