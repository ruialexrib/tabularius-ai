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
    private const string StrongPassword="ChangedPass123!";
    /// <summary>Verifies bootstrap administrators are redirected while the temporary password remains active.</summary>
    [Fact] public async Task InvokeAsync_BootstrapPassword_RedirectsToChangePassword(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var u=await CreateBootstrapAdministratorAsync(db,id);var context=CreateContext(u.Id,"/");var called=false;await new MandatoryPasswordChangeMiddleware(_=>{called=true;return Task.CompletedTask;}).InvokeAsync(context,id.UserManager);Assert.Equal("/Account/ChangePassword",context.Response.Headers.Location);Assert.False(called);}
    /// <summary>Verifies the password change endpoint remains reachable.</summary>
    [Fact] public async Task InvokeAsync_ChangePasswordPath_AllowsRequest(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var u=await CreateBootstrapAdministratorAsync(db,id);Assert.True(await InvokeAndReturnNextCalled(id,CreateContext(u.Id,"/Account/ChangePassword")));}
    /// <summary>Verifies logout remains reachable while replacement is mandatory.</summary>
    [Fact] public async Task InvokeAsync_LogoutPath_AllowsRequest(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var u=await CreateBootstrapAdministratorAsync(db,id);Assert.True(await InvokeAndReturnNextCalled(id,CreateContext(u.Id,"/Account/Logout")));}
    /// <summary>Verifies anonymous requests pass through unchanged.</summary>
    [Fact] public async Task InvokeAsync_AnonymousRequest_AllowsRequest(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var context=new DefaultHttpContext();context.Request.Path="/";Assert.True(await InvokeAndReturnNextCalled(id,context));}
    /// <summary>Verifies a non-bootstrap authenticated user passes through.</summary>
    [Fact] public async Task InvokeAsync_NormalUser_AllowsRequest(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var u=await id.CreateUserAsync("rui",StrongPassword);Assert.True(await InvokeAndReturnNextCalled(id,CreateContext(u.Id,"/")));}
    /// <summary>Verifies administrators with replaced passwords pass through.</summary>
    [Fact] public async Task InvokeAsync_ReplacedPassword_AllowsRequest(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var u=await id.CreateUserAsync("admin",StrongPassword,ApplicationRoles.Administrator);Assert.True(await InvokeAndReturnNextCalled(id,CreateContext(u.Id,"/")));}
    /// <summary>Verifies an authenticated principal whose Identity account no longer exists passes through.</summary>
    [Fact] public async Task InvokeAsync_UnknownAuthenticatedPrincipal_AllowsRequest(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);Assert.True(await InvokeAndReturnNextCalled(id,CreateContext("missing-user","/")));}

    /// <summary>Invokes middleware and reports whether the downstream delegate executed.</summary>
    private static async Task<bool> InvokeAndReturnNextCalled(TestIdentityServices identity,DefaultHttpContext context){var called=false;await new MandatoryPasswordChangeMiddleware(_=>{called=true;return Task.CompletedTask;}).InvokeAsync(context,identity.UserManager);return called;}
    /// <summary>Creates the exceptional bootstrap credential without weakening normal password policy.</summary>
    private static async Task<ApplicationUser> CreateBootstrapAdministratorAsync(TestDatabase database,TestIdentityServices identity){var u=await identity.CreateUserAsync("admin",StrongPassword,ApplicationRoles.Administrator);u.PasswordHash=new PasswordHasher<ApplicationUser>().HashPassword(u,"LetMeIn");database.Context.Users.Update(u);await database.Context.SaveChangesAsync();database.Context.ChangeTracker.Clear();return await database.Context.Users.SingleAsync(x=>x.Id==u.Id);}
    /// <summary>Creates an authenticated HTTP context whose identifier resolves through Identity.</summary>
    private static DefaultHttpContext CreateContext(string userId,string path){var context=new DefaultHttpContext();context.Request.Path=path;context.User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,userId)},"Tests"));return context;}
}
