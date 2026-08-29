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
    /// <summary>Displays the login page.</summary>
    /// <param name="returnUrl">The local URL to return to after authentication.</param>
    /// <returns>The login view or the application home page when already authenticated.</returns>
    [AllowAnonymous, HttpGet]
    public IActionResult Login(string? returnUrl = null) => User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Home") : View(CreateLoginModel(returnUrl));

    /// <summary>Authenticates an existing local account.</summary>
    /// <param name="model">The submitted credentials.</param>
    /// <returns>A redirect on success or the login view with validation errors.</returns>
    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        model.GoogleEnabled = IsGoogleConfigured;
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.FindByEmailAsync(model.Email.Trim());
        if (user is null) { ModelState.AddModelError(string.Empty, "Email ou palavra-passe inválidos."); return View(model); }
        var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded) { ModelState.AddModelError(string.Empty, result.IsLockedOut ? "Conta temporariamente bloqueada." : "Email ou palavra-passe inválidos."); return View(model); }
        return RedirectAfterLogin(model.ReturnUrl);
    }

    /// <summary>Starts authentication with the configured Google provider.</summary>
    /// <param name="provider">The external authentication provider.</param>
    /// <param name="returnUrl">The local URL to return to after authentication.</param>
    /// <returns>An external authentication challenge.</returns>
    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        if (!IsGoogleConfigured || !string.Equals(provider, "Google", StringComparison.Ordinal)) return NotFound();
        var callbackUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        return Challenge(signInManager.ConfigureExternalAuthenticationProperties(provider, callbackUrl), provider);
    }

    /// <summary>Signs in an existing account whose email matches the verified Google identity.</summary>
    /// <param name="returnUrl">The local URL to return to after authentication.</param>
    /// <param name="remoteError">An optional error returned by the external provider.</param>
    /// <returns>A redirect on success or the login view with an error.</returns>
    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (!IsGoogleConfigured) return NotFound();
        if (!string.IsNullOrWhiteSpace(remoteError)) return ExternalLoginError(returnUrl, "O Google não conseguiu concluir a autenticação.");
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null || !string.Equals(info.LoginProvider, "Google", StringComparison.Ordinal)) return ExternalLoginError(returnUrl, "Não foi possível validar a resposta do Google.");
        var email = info.Principal.FindFirstValue(ClaimTypes.Email)?.Trim();
        if (string.IsNullOrWhiteSpace(email)) return ExternalLoginError(returnUrl, "A conta Google não disponibilizou um endereço de email.");
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return ExternalLoginError(returnUrl, "Este email não está autorizado. Solicite ao administrador a criação da sua conta.");
        if (await userManager.IsLockedOutAsync(user)) return ExternalLoginError(returnUrl, "A conta encontra-se temporariamente bloqueada.");
        await signInManager.SignInAsync(user, isPersistent: false, authenticationMethod: info.LoginProvider);
        return RedirectAfterLogin(returnUrl);
    }

    /// <summary>Signs out the current user.</summary>
    /// <returns>A redirect to the login page.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout() { await signInManager.SignOutAsync(); return RedirectToAction(nameof(Login)); }

    /// <summary>Displays the access denied page.</summary>
    /// <returns>The access denied view.</returns>
    [AllowAnonymous, HttpGet]
    public IActionResult AccessDenied() => View();

    /// <summary>Creates the login view model for the current configuration.</summary>
    private LoginViewModel CreateLoginModel(string? returnUrl) => new() { ReturnUrl = returnUrl, GoogleEnabled = IsGoogleConfigured };

    /// <summary>Gets whether Google authentication credentials are configured.</summary>
    private bool IsGoogleConfigured => !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"]) && !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientSecret"]);

    /// <summary>Returns a safe post-authentication redirect.</summary>
    private IActionResult RedirectAfterLogin(string? returnUrl) => Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Home");

    /// <summary>Returns the login page with an external authentication error.</summary>
    private IActionResult ExternalLoginError(string? returnUrl, string message) { ModelState.AddModelError(string.Empty, message); return View(nameof(Login), CreateLoginModel(returnUrl)); }
}
