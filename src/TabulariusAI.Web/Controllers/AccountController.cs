using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TabulariusAI.Web.Data.Identity;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Controllers;

/// <summary>Handles local and external authentication for Tabularius AI.</summary>
public sealed class AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IConfiguration configuration) : Controller
{
    private const string BootstrapUserName = "admin";
    private const string BootstrapPassword = "LetMeIn";

    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(await CreateLoginModelAsync(returnUrl));
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        await PopulateLoginStateAsync(model);
        if (!ModelState.IsValid) return View(model);
        var identifier = model.Identifier.Trim();
        var user = await userManager.FindByNameAsync(identifier) ?? await userManager.FindByEmailAsync(identifier);
        if (user is null) return InvalidCredentials(model);
        var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.IsLockedOut ? "Conta temporariamente bloqueada." : "Utilizador ou palavra-passe inválidos.");
            return View(model);
        }
        if (await MustReplaceBootstrapPasswordAsync(user)) return RedirectToAction(nameof(ChangePassword));
        return RedirectAfterLogin(model.ReturnUrl);
    }

    /// <summary>Displays the current user's preferences.</summary>
    [HttpGet]
    public async Task<IActionResult> Preferences()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));
        return View(user);
    }

    /// <summary>Displays the password change page.</summary>
    [HttpGet]
    public async Task<IActionResult> ChangePassword()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));
        ViewData["MandatoryPasswordChange"] = await MustReplaceBootstrapPasswordAsync(user);
        return View(new ChangePasswordViewModel());
    }

    /// <summary>Changes the authenticated user's local password.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));
        var mandatory = await MustReplaceBootstrapPasswordAsync(user);
        ViewData["MandatoryPasswordChange"] = mandatory;
        if (!ModelState.IsValid) return View(model);
        if (!await userManager.HasPasswordAsync(user))
        {
            ModelState.AddModelError(string.Empty, "Esta conta não tem uma palavra-passe local configurada.");
            return View(model);
        }
        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, TranslatePasswordError(error));
            return View(model);
        }
        await signInManager.RefreshSignInAsync(user);
        TempData["SuccessMessage"] = "Palavra-passe alterada com sucesso.";
        return mandatory ? RedirectToAction("Index", "Home") : RedirectToAction(nameof(Preferences));
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        if (!IsGoogleConfigured || !string.Equals(provider, "Google", StringComparison.Ordinal)) return NotFound();
        var callbackUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        return Challenge(signInManager.ConfigureExternalAuthenticationProperties(provider, callbackUrl), provider);
    }

    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (!IsGoogleConfigured) return NotFound();
        if (!string.IsNullOrWhiteSpace(remoteError)) return await ExternalLoginErrorAsync(returnUrl, "O Google não conseguiu concluir a autenticação.");
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null || !string.Equals(info.LoginProvider, "Google", StringComparison.Ordinal)) return await ExternalLoginErrorAsync(returnUrl, "Não foi possível validar a resposta do Google.");
        var email = info.Principal.FindFirstValue(ClaimTypes.Email)?.Trim();
        if (string.IsNullOrWhiteSpace(email)) return await ExternalLoginErrorAsync(returnUrl, "A conta Google não disponibilizou um endereço de email.");
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return await ExternalLoginErrorAsync(returnUrl, "Este email não está autorizado. Solicite ao administrador a criação da sua conta.");
        if (await userManager.IsLockedOutAsync(user)) return await ExternalLoginErrorAsync(returnUrl, "A conta encontra-se temporariamente bloqueada.");
        await signInManager.SignInAsync(user, isPersistent: false, authenticationMethod: info.LoginProvider);
        return RedirectAfterLogin(returnUrl);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout() { await signInManager.SignOutAsync(); return RedirectToAction(nameof(Login)); }

    [AllowAnonymous, HttpGet]
    public IActionResult AccessDenied() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptCookieConsent(string? returnUrl = null)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        user.CookieConsentAcceptedAt = DateTimeOffset.UtcNow;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded) TempData["ErrorMessage"] = "Não foi possível guardar o consentimento de cookies.";
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Home");
    }

    private async Task<bool> MustReplaceBootstrapPasswordAsync(ApplicationUser user) => string.Equals(user.UserName, BootstrapUserName, StringComparison.OrdinalIgnoreCase) && await userManager.CheckPasswordAsync(user, BootstrapPassword);
    private async Task<bool> ShouldShowBootstrapCredentialsAsync(){var user=await userManager.FindByNameAsync(BootstrapUserName);return user is not null&&await MustReplaceBootstrapPasswordAsync(user);}
    private async Task PopulateLoginStateAsync(LoginViewModel model){model.GoogleEnabled=IsGoogleConfigured;model.ShowBootstrapCredentials=await ShouldShowBootstrapCredentialsAsync();}
    private IActionResult InvalidCredentials(LoginViewModel model){ModelState.AddModelError(string.Empty,"Utilizador ou palavra-passe inválidos.");return View(model);}
    private static string TranslatePasswordError(IdentityError error)=>error.Code switch{"PasswordTooShort"=>"A nova palavra-passe deve ter pelo menos 12 caracteres.","PasswordRequiresDigit"=>"A nova palavra-passe deve incluir pelo menos um algarismo.","PasswordRequiresLower"=>"A nova palavra-passe deve incluir pelo menos uma letra minúscula.","PasswordRequiresUpper"=>"A nova palavra-passe deve incluir pelo menos uma letra maiúscula.","PasswordRequiresNonAlphanumeric"=>"A nova palavra-passe deve incluir pelo menos um carácter especial.","PasswordMismatch"=>"A palavra-passe atual está incorreta.",_=>"Não foi possível alterar a palavra-passe. Verifique os dados introduzidos."};
    private async Task<LoginViewModel> CreateLoginModelAsync(string? returnUrl){var model=new LoginViewModel{ReturnUrl=returnUrl};await PopulateLoginStateAsync(model);return model;}
    private bool IsGoogleConfigured=>!string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"])&&!string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientSecret"]);
    private IActionResult RedirectAfterLogin(string? returnUrl)=>Url.IsLocalUrl(returnUrl)?LocalRedirect(returnUrl):RedirectToAction("Index","Home");
    private async Task<IActionResult> ExternalLoginErrorAsync(string? returnUrl,string message){ModelState.AddModelError(string.Empty,message);return View(nameof(Login),await CreateLoginModelAsync(returnUrl));}
}
