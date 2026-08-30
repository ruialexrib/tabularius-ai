using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
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

    /// <summary>Verifies invalid credentials return a validation error.</summary>
    [Fact] public async Task Login_InvalidCredentials_ReturnsFormError(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var c=CreateController(id);Assert.IsType<ViewResult>(await c.Login(new LoginViewModel{Identifier="missing",Password=StrongPassword}));Assert.False(c.ModelState.IsValid);}
    /// <summary>Verifies valid credentials redirect to a local return URL.</summary>
    [Fact] public async Task Login_ValidCredentials_RedirectsLocally(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);await id.CreateUserAsync("rui",StrongPassword);var r=Assert.IsType<LocalRedirectResult>(await CreateController(id).Login(new LoginViewModel{Identifier="rui",Password=StrongPassword,ReturnUrl="/Dossier/Index"}));Assert.Equal("/Dossier/Index",r.Url);}
    /// <summary>Verifies email addresses resolve local accounts.</summary>
    [Fact] public async Task Login_EmailAddress_ResolvesUserName(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var u=await id.CreateUserAsync("emailuser",StrongPassword);u.Email="email@example.test";Assert.True((await id.UserManager.UpdateAsync(u)).Succeeded);var r=Assert.IsType<RedirectToActionResult>(await CreateController(id).Login(new LoginViewModel{Identifier="email@example.test",Password=StrongPassword}));Assert.Equal("Home",r.ControllerName);}
    /// <summary>Verifies locked accounts cannot authenticate.</summary>
    [Fact] public async Task Login_LockedAccount_ReturnsFormError(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var u=await id.CreateUserAsync("locked",StrongPassword);Assert.True((await id.UserManager.SetLockoutEndDateAsync(u,DateTimeOffset.UtcNow.AddDays(1))).Succeeded);var c=CreateController(id);Assert.IsType<ViewResult>(await c.Login(new LoginViewModel{Identifier="locked",Password=StrongPassword}));Assert.False(c.ModelState.IsValid);}
    /// <summary>Verifies invalid model state bypasses authentication.</summary>
    [Fact] public async Task Login_InvalidModelState_ReturnsPopulatedModel(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var c=CreateController(id);c.ModelState.AddModelError("Identifier","required");var model=new LoginViewModel{Identifier="",Password=""};Assert.Same(model,Assert.IsType<ViewResult>(await c.Login(model)).Model);}
    /// <summary>Verifies login GET preserves return URL and exposes bootstrap state.</summary>
    [Fact] public async Task Login_GetBootstrapAccount_ExposesTemporaryCredentialState(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);await id.CreateUserAsync("admin","LetMeIn",ApplicationRoles.Administrator);var model=Assert.IsType<LoginViewModel>(Assert.IsType<ViewResult>(await CreateController(id).Login("/Dossier/Index")).Model);Assert.True(model.ShowBootstrapCredentials);Assert.Equal("/Dossier/Index",model.ReturnUrl);}
    /// <summary>Verifies authenticated users do not see the login page.</summary>
    [Fact] public async Task Login_GetAuthenticatedUser_RedirectsHome(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var u=await id.CreateUserAsync("rui",StrongPassword);var r=Assert.IsType<RedirectToActionResult>(await CreateController(id,u).Login((string?)null));Assert.Equal("Home",r.ControllerName);}
    /// <summary>Verifies bootstrap credentials require password replacement.</summary>
    [Fact] public async Task Login_BootstrapTemporaryPassword_RedirectsToChangePassword(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);await id.CreateUserAsync("admin","LetMeIn",ApplicationRoles.Administrator);var r=Assert.IsType<RedirectToActionResult>(await CreateController(id).Login(new LoginViewModel{Identifier="admin",Password="LetMeIn"}));Assert.Equal("ChangePassword",r.ActionName);}
    /// <summary>Verifies anonymous password-change requests redirect to login.</summary>
    [Fact] public async Task ChangePassword_GetAnonymous_RedirectsLogin(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var r=Assert.IsType<RedirectToActionResult>(await CreateController(id).ChangePassword());Assert.Equal("Login",r.ActionName);}
    /// <summary>Verifies non-bootstrap users cannot access password replacement.</summary>
    [Fact] public async Task ChangePassword_Get_NonBootstrapUser_RedirectsHome(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var u=await id.CreateUserAsync("rui",StrongPassword);var r=Assert.IsType<RedirectToActionResult>(await CreateController(id,u).ChangePassword());Assert.Equal("Home",r.ControllerName);}
    /// <summary>Verifies bootstrap users can open the replacement form.</summary>
    [Fact] public async Task ChangePassword_GetBootstrapUser_ReturnsForm(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var u=await id.CreateUserAsync("admin","LetMeIn",ApplicationRoles.Administrator);Assert.IsType<ChangePasswordViewModel>(Assert.IsType<ViewResult>(await CreateController(id,u).ChangePassword()).Model);}
    /// <summary>Verifies valid bootstrap password replacement.</summary>
    [Fact] public async Task ChangePassword_ValidReplacement_ChangesCredential(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var u=await id.CreateUserAsync("admin","LetMeIn",ApplicationRoles.Administrator);var r=await CreateController(id,u).ChangePassword(new ChangePasswordViewModel{CurrentPassword="LetMeIn",NewPassword=StrongPassword,ConfirmPassword=StrongPassword});Assert.IsType<RedirectToActionResult>(r);Assert.True(await id.UserManager.CheckPasswordAsync(u,StrongPassword));}
    /// <summary>Verifies wrong current passwords leave bootstrap credentials unchanged.</summary>
    [Fact] public async Task ChangePassword_WrongCurrentPassword_ReturnsFormError(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var u=await id.CreateUserAsync("admin","LetMeIn",ApplicationRoles.Administrator);var c=CreateController(id,u);Assert.IsType<ViewResult>(await c.ChangePassword(new ChangePasswordViewModel{CurrentPassword="wrong",NewPassword=StrongPassword,ConfirmPassword=StrongPassword}));Assert.False(c.ModelState.IsValid);}
    /// <summary>Verifies password-policy failures are translated into form errors.</summary>
    [Fact] public async Task ChangePassword_WeakReplacement_ReturnsFormError(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var u=await id.CreateUserAsync("admin","LetMeIn",ApplicationRoles.Administrator);var c=CreateController(id,u);Assert.IsType<ViewResult>(await c.ChangePassword(new ChangePasswordViewModel{CurrentPassword="LetMeIn",NewPassword="weak",ConfirmPassword="weak"}));Assert.False(c.ModelState.IsValid);Assert.True(await id.UserManager.CheckPasswordAsync(u,"LetMeIn"));}
    /// <summary>Verifies invalid password-change model state returns without changing credentials.</summary>
    [Fact] public async Task ChangePassword_InvalidModelState_ReturnsForm(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var u=await id.CreateUserAsync("admin","LetMeIn",ApplicationRoles.Administrator);var c=CreateController(id,u);c.ModelState.AddModelError("NewPassword","invalid");Assert.IsType<ViewResult>(await c.ChangePassword(new ChangePasswordViewModel{CurrentPassword="LetMeIn",NewPassword=StrongPassword,ConfirmPassword=StrongPassword}));Assert.True(await id.UserManager.CheckPasswordAsync(u,"LetMeIn"));}
    /// <summary>Verifies password-change POST for anonymous users redirects to login.</summary>
    [Fact] public async Task ChangePassword_PostAnonymous_RedirectsLogin(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var r=Assert.IsType<RedirectToActionResult>(await CreateController(id).ChangePassword(new ChangePasswordViewModel{CurrentPassword="x",NewPassword=StrongPassword,ConfirmPassword=StrongPassword}));Assert.Equal("Login",r.ActionName);}
    /// <summary>Verifies unconfigured external authentication returns not found.</summary>
    [Fact] public async Task ExternalLogin_GoogleNotConfigured_ReturnsNotFound(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);Assert.IsType<NotFoundResult>(CreateController(id).ExternalLogin("Google"));Assert.IsType<NotFoundResult>(await CreateController(id).ExternalLoginCallback());}
    /// <summary>Verifies unsupported external providers return not found even when Google is configured.</summary>
    [Fact] public async Task ExternalLogin_UnsupportedProvider_ReturnsNotFound(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);Assert.IsType<NotFoundResult>(CreateController(id,configuration:GoogleConfiguration()).ExternalLogin("Microsoft"));}
    /// <summary>Verifies remote external authentication errors return the login form.</summary>
    [Fact] public async Task ExternalLoginCallback_RemoteError_ReturnsLoginError(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var c=CreateController(id,configuration:GoogleConfiguration());var r=Assert.IsType<ViewResult>(await c.ExternalLoginCallback("/","access_denied"));Assert.Equal("Login",r.ViewName);Assert.False(c.ModelState.IsValid);}
    /// <summary>Verifies missing external login information returns the login form.</summary>
    [Fact] public async Task ExternalLoginCallback_MissingInfo_ReturnsLoginError(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var c=CreateController(id,configuration:GoogleConfiguration());var r=Assert.IsType<ViewResult>(await c.ExternalLoginCallback());Assert.Equal("Login",r.ViewName);Assert.False(c.ModelState.IsValid);}
    /// <summary>Verifies logout redirects to login.</summary>
    [Fact] public async Task Logout_RedirectsLogin(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);var r=Assert.IsType<RedirectToActionResult>(await CreateController(id).Logout());Assert.Equal("Login",r.ActionName);}
    /// <summary>Verifies access denied returns its view.</summary>
    [Fact] public async Task AccessDenied_ReturnsView(){await using var db=new TestDatabase();await using var id=new TestIdentityServices(db.Context);Assert.IsType<ViewResult>(CreateController(id).AccessDenied());}

    /// <summary>Creates an account controller sharing the Identity HTTP and MVC routing context.</summary>
    private static AccountController CreateController(TestIdentityServices identity,ApplicationUser? currentUser=null,IConfiguration? configuration=null){var context=identity.HttpContext;context.User=currentUser is null?new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity()):new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]{new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier,currentUser.Id)},"Tests"));configuration??=new ConfigurationBuilder().AddInMemoryCollection().Build();var actionContext=new ActionContext(context,new RouteData(),new ActionDescriptor());var controller=new AccountController(identity.SignInManager,identity.UserManager,configuration){ControllerContext=new ControllerContext(actionContext)};controller.TempData=new TempDataDictionary(context,new TestTempDataProvider());return controller;}
    /// <summary>Creates configuration with Google authentication enabled.</summary>
    private static IConfiguration GoogleConfiguration()=>new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Authentication:Google:ClientId","client"},{"Authentication:Google:ClientSecret","secret"}}).Build();
    /// <summary>Provides isolated TempData storage.</summary>
    private sealed class TestTempDataProvider:ITempDataProvider{/// <summary>Loads empty TempData.</summary>
        public IDictionary<string,object> LoadTempData(Microsoft.AspNetCore.Http.HttpContext context)=>new Dictionary<string,object>();/// <summary>Accepts TempData writes.</summary>
        public void SaveTempData(Microsoft.AspNetCore.Http.HttpContext context,IDictionary<string,object> values){} }
}
