using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TabulariusAI.Web.Controllers;
using TabulariusAI.Web.Data.Identity;
using TabulariusAI.Web.Models;
using TabulariusAI.Web.Tests.Infrastructure;
using Xunit;

namespace TabulariusAI.Web.Tests.Controllers;

/// <summary>Verifies administrator user creation, editing, password reset, filtering and lock behavior.</summary>
public sealed class UsersControllerTests
{
    private const string StrongPassword = "ValidPass123!";
    private const string NewPassword = "AnotherPass456!";

    /// <summary>Verifies that creating a valid user persists the account and selected role.</summary>
    [Fact]
    public async Task Create_ValidUser_CreatesAccountAndRole()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var controller = CreateController(identity, null);
        var model = new CreateUserViewModel { UserName = "maria", DisplayName = "Maria Silva", Email = "maria@example.test", Password = StrongPassword, ConfirmPassword = StrongPassword, Role = ApplicationRoles.User };

        var result = await controller.Create(model);

        Assert.IsType<RedirectToActionResult>(result);
        var user = await identity.UserManager.FindByNameAsync("maria");
        Assert.NotNull(user);
        Assert.True(await identity.UserManager.IsInRoleAsync(user!, ApplicationRoles.User));
    }

    /// <summary>Verifies that an unsupported application role is rejected without creating an account.</summary>
    [Fact]
    public async Task Create_InvalidRole_ReturnsFormWithoutCreatingUser()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var controller = CreateController(identity, null);
        var model = new CreateUserViewModel { UserName = "maria", DisplayName = "Maria", Email = "maria@example.test", Password = StrongPassword, ConfirmPassword = StrongPassword, Role = "Owner" };

        var result = await controller.Create(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Null(await identity.UserManager.FindByNameAsync("maria"));
    }

    /// <summary>Verifies that editing an account updates its profile and application role.</summary>
    [Fact]
    public async Task Edit_ValidChange_UpdatesProfileAndRole()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var admin = await identity.CreateUserAsync("admin2", StrongPassword, ApplicationRoles.Administrator);
        var user = await identity.CreateUserAsync("joao", StrongPassword);
        var controller = CreateController(identity, admin);
        var model = new EditUserViewModel { Id = user.Id, UserName = "joao.novo", DisplayName = "João Novo", Email = "joao.novo@example.test", Role = ApplicationRoles.Administrator };

        var result = await controller.Edit(model);

        Assert.IsType<RedirectToActionResult>(result);
        var updated = await identity.UserManager.FindByIdAsync(user.Id);
        Assert.Equal("joao.novo", updated!.UserName);
        Assert.Equal("João Novo", updated.DisplayName);
        Assert.True(await identity.UserManager.IsInRoleAsync(updated, ApplicationRoles.Administrator));
    }

    /// <summary>Verifies that the signed-in administrator cannot remove their own administrator role.</summary>
    [Fact]
    public async Task Edit_CurrentAdministratorDemotion_IsRejected()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var admin = await identity.CreateUserAsync("admin2", StrongPassword, ApplicationRoles.Administrator);
        var controller = CreateController(identity, admin);
        var model = new EditUserViewModel { Id = admin.Id, UserName = admin.UserName!, DisplayName = admin.DisplayName, Email = admin.Email!, Role = ApplicationRoles.User };

        var result = await controller.Edit(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(await identity.UserManager.IsInRoleAsync(admin, ApplicationRoles.Administrator));
    }

    /// <summary>Verifies that an administrator can reset another user's password.</summary>
    [Fact]
    public async Task ResetPassword_ValidPassword_ReplacesCredential()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var admin = await identity.CreateUserAsync("admin2", StrongPassword, ApplicationRoles.Administrator);
        var user = await identity.CreateUserAsync("ana", StrongPassword);
        var controller = CreateController(identity, admin);
        var model = new ResetUserPasswordViewModel { Id = user.Id, NewPassword = NewPassword, ConfirmPassword = NewPassword };

        var result = await controller.ResetPassword(model);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.True(await identity.UserManager.CheckPasswordAsync(user, NewPassword));
        Assert.False(await identity.UserManager.CheckPasswordAsync(user, StrongPassword));
    }

    /// <summary>Verifies that administrators cannot lock the account used for the current session.</summary>
    [Fact]
    public async Task ToggleLock_CurrentUser_DoesNotLockAccount()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var admin = await identity.CreateUserAsync("admin2", StrongPassword, ApplicationRoles.Administrator);
        var controller = CreateController(identity, admin);

        var result = await controller.ToggleLock(admin.Id);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.False(await identity.UserManager.IsLockedOutAsync(admin));
    }

    /// <summary>Verifies that locking and unlocking another account toggles its effective lock state.</summary>
    [Fact]
    public async Task ToggleLock_OtherUser_TogglesLockState()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var admin = await identity.CreateUserAsync("admin2", StrongPassword, ApplicationRoles.Administrator);
        var user = await identity.CreateUserAsync("pedro", StrongPassword);
        var controller = CreateController(identity, admin);

        await controller.ToggleLock(user.Id);
        Assert.True(await identity.UserManager.IsLockedOutAsync(user));
        await controller.ToggleLock(user.Id);
        Assert.False(await identity.UserManager.IsLockedOutAsync(user));
    }

    /// <summary>Verifies search and role filtering and page-size normalization on the administration list.</summary>
    [Fact]
    public async Task Index_SearchAndRole_ReturnsMatchingNormalizedPage()
    {
        await using var database = new TestDatabase();
        await using var identity = new TestIdentityServices(database.Context);
        var admin = await identity.CreateUserAsync("admin2", StrongPassword, ApplicationRoles.Administrator);
        var user = await identity.CreateUserAsync("searchable", StrongPassword);
        user.DisplayName = "Pessoa Pesquisável";
        Assert.True((await identity.UserManager.UpdateAsync(user)).Succeeded);
        var controller = CreateController(identity, admin);

        var result = await controller.Index("Pesquisável", ApplicationRoles.User, "active", -2, 999);

        var model = Assert.IsType<UserListViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(1, model.List.Page);
        Assert.Equal(10, model.List.PageSize);
        Assert.Single(model.List.Items);
        Assert.Equal("searchable", model.List.Items[0].UserName);
    }

    /// <summary>Creates a controller with authenticated claims and in-memory TempData.</summary>
    private static UsersController CreateController(TestIdentityServices identity, ApplicationUser? currentUser)
    {
        var context = new DefaultHttpContext();
        if (currentUser is not null) context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, currentUser.Id) }, "Tests"));
        var controller = new UsersController(identity.UserManager) { ControllerContext = new ControllerContext { HttpContext = context } };
        controller.TempData = new TempDataDictionary(context, new TestTempDataProvider());
        return controller;
    }

    /// <summary>Provides isolated TempData storage for controller tests.</summary>
    private sealed class TestTempDataProvider : ITempDataProvider
    {
        /// <summary>Loads empty TempData.</summary>
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        /// <summary>Accepts TempData writes without external persistence.</summary>
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
