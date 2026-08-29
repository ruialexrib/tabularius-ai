using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Data.Identity;
using Xunit;

namespace TabulariusAI.Web.Tests.Infrastructure;

/// <summary>Provides real ASP.NET Core Identity services backed by the isolated relational test database.</summary>
public sealed class TestIdentityServices : IAsyncDisposable
{
    private readonly ServiceProvider provider;

    /// <summary>Initializes Identity with the same relevant password and lockout rules used by the application.</summary>
    public TestIdentityServices(TabulariusDbContext context)
    {
        var services = new ServiceCollection();
        services.AddLogging();
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
    }

    /// <summary>Gets the configured application user manager.</summary>
    public UserManager<ApplicationUser> UserManager => provider.GetRequiredService<UserManager<ApplicationUser>>();

    /// <summary>Gets the configured application sign-in manager.</summary>
    public SignInManager<ApplicationUser> SignInManager => provider.GetRequiredService<SignInManager<ApplicationUser>>();

    /// <summary>Creates a valid application user with the supplied credentials.</summary>
    public async Task<ApplicationUser> CreateUserAsync(string userName, string password, string role = ApplicationRoles.User)
    {
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync(role)) Assert.True((await roleManager.CreateAsync(new IdentityRole(role))).Succeeded);
        var user = new ApplicationUser { UserName = userName, Email = $"{userName}@tabularius.local", EmailConfirmed = true, DisplayName = userName, LockoutEnabled = true };
        Assert.True((await UserManager.CreateAsync(user, password)).Succeeded);
        Assert.True((await UserManager.AddToRoleAsync(user, role)).Succeeded);
        return user;
    }

    /// <summary>Disposes the Identity service provider.</summary>
    public ValueTask DisposeAsync()
    {
        provider.Dispose();
        return ValueTask.CompletedTask;
    }
}
