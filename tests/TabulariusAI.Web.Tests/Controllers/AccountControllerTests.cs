using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using TabulariusAI.Web.Controllers;
using TabulariusAI.Web.Data.Identity;
using TabulariusAI.Web.Models;
using TabulariusAI.Web.Tests.Infrastructure;
using Xunit;

namespace TabulariusAI.Web.Tests.Controllers;

/// <summary>Verifies local account authentication and mandatory bootstrap password-change workflows.</summary>
public sealed class AccountControllerTests
{
    private const string StrongPassword = "ValidPass123!";

    /// <summary>Verifies that invalid credentials return the login form with a validation error.</summary>
    [Fact]
    public async Task Login_InvalidCredentials_ReturnsFormError()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var controller = CreateController(identity);
        var result = await controller.Login(new LoginViewModel { Identifier = "missing", Password = StrongPassword });
        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    /// <summary>Verifies that a valid local account signs in and redirects to the requested local URL.</summary>
    [Fact]
    public async Task Login_ValidCredentials_RedirectsLocally()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        await identity.CreateUserAsync("rui", StrongPassword);
        var controller = CreateController(identity);
        var result = await controller.Login(new LoginViewModel { Identifier = "rui", Password = StrongPassword, ReturnUrl = "/Dossier/Index" });
        var redirect = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/Dossier/Index", redirect.Url);
    }

    /// <summary>Verifies that email addresses can be used to resolve a local account during sign-in.</summary>
    [Fact]
    public async Task Login_EmailAddress_ResolvesUserName()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var user = await identity.CreateUserAsync("emailuser", StrongPassword);
        user.Email = "email@example.test";
        Assert.True((await identity.UserManager.UpdateAsync(user)).Succeeded);
        var controller = CreateController(identity);
        var result = await controller.Login(new LoginViewModel { Identifier = "email@example.test", Password = StrongPassword });
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    /// <summary>Verifies that a locked account cannot authenticate.</summary>
    [Fact]
    public async Task Login_LockedAccount_ReturnsFormError()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var user = await identity.CreateUserAsync("locked", StrongPassword);
        Assert.True((await identity.UserManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddDays(1))).Succeeded);
        var controller = CreateController(identity);
        var result = await controller.Login(new LoginViewModel { Identifier = "locked", Password = StrongPassword });
        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    /// <summary>Verifies that a bootstrap administrator using the temporary password is redirected to password change.</summary>
    [Fact]
    public async Task Login_BootstrapTemporaryPassword_RedirectsToChangePassword()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        await identity.CreateUserAsync("admin", "LetMeIn", ApplicationRoles.Administrator);
        var controller = CreateController(identity);
        var result = await controller.Login(new LoginViewModel { Identifier = "admin", Password = "LetMeIn" });
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ChangePassword", redirect.ActionName);
    }

    /// <summary>Verifies that password change is unavailable to users who are not using the bootstrap credential.</summary>
    [Fact]
    public async Task ChangePassword_Get_NonBootstrapUser_RedirectsHome()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var user = await identity.CreateUserAsync("rui", StrongPassword);
        var controller = CreateController(identity, user);
        var result = await controller.ChangePassword();
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    /// <summary>Verifies that the bootstrap password can be replaced with a compliant permanent password.</summary>
    [Fact]
    public async Task ChangePassword_ValidReplacement_ChangesCredential()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var user = await identity.CreateUserAsync("admin", "LetMeIn", ApplicationRoles.Administrator);
        var controller = CreateController(identity, user);
        var result = await controller.ChangePassword(new ChangePasswordViewModel { CurrentPassword = "LetMeIn", NewPassword = StrongPassword, ConfirmPassword = StrongPassword });
        Assert.IsType<RedirectToActionResult>(result);
        Assert.True(await identity.UserManager.CheckPasswordAsync(user, StrongPassword));
        Assert.False(await identity.UserManager.CheckPasswordAsync(user, "LetMeIn"));
    }

    /// <summary>Verifies that a wrong current password leaves the bootstrap credential unchanged.</summary>
    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsFormError()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var user = await identity.CreateUserAsync("admin", "LetMeIn", ApplicationRoles.Administrator);
        var controller = CreateController(identity, user);
        var result = await controller.ChangePassword(new ChangePasswordViewModel { CurrentPassword = "wrong", NewPassword = StrongPassword, ConfirmPassword = StrongPassword });
        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(await identity.UserManager.CheckPasswordAsync(user, "LetMeIn"));
    }

    /// <summary>Creates an account controller using the same HTTP context as the real Identity sign-in manager.</summary>
    private static AccountController CreateController(TestIdentityServices identity, ApplicationUser? currentUser = null)
    {
        var context = identity.HttpContext;
        if (currentUser is not null) context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, currentUser.Id) }, "Tests"));
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var controller = new AccountController(identity.SignInManager, identity.UserManager, configuration) { ControllerContext = new ControllerContext { HttpContext = context } };
        controller.TempData = new TempDataDictionary(context, new TestTempDataProvider());
        return controller;
    }

    /// <summary>Provides isolated TempData storage for account controller tests.</summary>
    private sealed class TestTempDataProvider : ITempDataProvider
    {
        /// <summary>Loads empty TempData.</summary>
        public IDictionary<string, object> LoadTempData(Microsoft.AspNetCore.Http.HttpContext context) => new Dictionary<string, object>();
        /// <summary>Accepts TempData writes without external persistence.</summary>
        public void SaveTempData(Microsoft.AspNetCore.Http.HttpContext context, IDictionary<string, object> values) { }
    }
}
