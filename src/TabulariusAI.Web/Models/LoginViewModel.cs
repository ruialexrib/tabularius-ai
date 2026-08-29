using System.ComponentModel.DataAnnotations;

namespace TabulariusAI.Web.Models;

/// <summary>Represents the credentials and options submitted from the login page.</summary>
public sealed class LoginViewModel
{
    /// <summary>Gets or sets the account username or email address.</summary>
    [Required(ErrorMessage = "Indique o utilizador.")]
    public string Identifier { get; set; } = string.Empty;

    /// <summary>Gets or sets the account password.</summary>
    [Required(ErrorMessage = "Indique a palavra-passe.")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the authentication cookie should persist.</summary>
    public bool RememberMe { get; set; }

    /// <summary>Gets or sets the local URL to return to after authentication.</summary>
    public string? ReturnUrl { get; set; }

    /// <summary>Gets or sets a value indicating whether Google authentication is configured.</summary>
    public bool GoogleEnabled { get; set; }

    /// <summary>Gets or sets a value indicating whether the local bootstrap administrator still uses the default credentials.</summary>
    public bool ShowBootstrapCredentials { get; set; }
}
