using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data.Identity;
using TabulariusAI.Web.Middleware;
using TabulariusAI.Web.Tests.Infrastructure;
using Xunit;

namespace TabulariusAI.Web.Tests.Middleware;

/// <summary>Verifies enforcement of the bootstrap administrator password replacement rule.</summary>
public sealed class MandatoryPasswordChangeMiddlewareTests
{
    private const string StrongPassword = "ChangedPass123!";

    /// <summary>Verifies that the authenticated bootstrap administrator is redirected while the temporary password remains active.</summary>
    [Fact]
    public async Task InvokeAsync_BootstrapPassword_RedirectsToChangePassword()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var user = await CreateBootstrapAdministratorAsync(database, identity);
        var context = CreateContext(user.Id, "/");
        var nextCalled = false;
        var middleware = new MandatoryPasswordChangeMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(context, identity.UserManager);

        Assert.Equal("/Account/ChangePassword", context.Response.Headers.Location);
        Assert.False(nextCalled);
    }

    /// <summary>Verifies that the password change endpoint remains reachable while replacement is mandatory.</summary>
    [Fact]
    public async Task InvokeAsync_ChangePasswordPath_AllowsRequest()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var user = await CreateBootstrapAdministratorAsync(database, identity);
        var context = CreateContext(user.Id, "/Account/ChangePassword");
        var nextCalled = false;
        var middleware = new MandatoryPasswordChangeMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(context, identity.UserManager);

        Assert.True(nextCalled);
    }

    /// <summary>Verifies that an administrator with a replaced password can access normal application routes.</summary>
    [Fact]
    public async Task InvokeAsync_ReplacedPassword_AllowsRequest()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var user = await identity.CreateUserAsync("admin", StrongPassword, ApplicationRoles.Administrator);
        var context = CreateContext(user.Id, "/");
        var nextCalled = false;
        var middleware = new MandatoryPasswordChangeMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(context, identity.UserManager);

        Assert.True(nextCalled);
    }

    /// <summary>Creates the exceptional bootstrap administrator credential without weakening the normal Identity password policy.</summary>
    private static async Task<ApplicationUser> CreateBootstrapAdministratorAsync(TestDatabase database, TestIdentityServices identity)
    {
        var user = await identity.CreateUserAsync("admin", StrongPassword, ApplicationRoles.Administrator);
        user.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(user, "LetMeIn");
        database.Context.Users.Update(user);
        await database.Context.SaveChangesAsync();
        database.Context.ChangeTracker.Clear();
        return await database.Context.Users.SingleAsync(item => item.Id == user.Id);
    }

    /// <summary>Creates an authenticated HTTP context whose name identifier resolves through Identity.</summary>
    private static DefaultHttpContext CreateContext(string userId, string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "Tests"));
        return context;
    }
}
