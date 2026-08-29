using System.ComponentModel.DataAnnotations;

namespace TabulariusAI.Web.Models;

/// <summary>Represents a mandatory password change request.</summary>
public sealed class ChangePasswordViewModel
{
    /// <summary>Gets or sets the current password.</summary>
    [Required(ErrorMessage = "Indique a palavra-passe atual.")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>Gets or sets the new password.</summary>
    [Required(ErrorMessage = "Indique a nova palavra-passe."), DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>Gets or sets the repeated new password.</summary>
    [Required(ErrorMessage = "Confirme a nova palavra-passe."), DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "As palavras-passe não coincidem.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
