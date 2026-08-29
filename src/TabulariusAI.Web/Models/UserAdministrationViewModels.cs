using System.ComponentModel.DataAnnotations;

namespace TabulariusAI.Web.Models;

/// <summary>Represents one user in the administrator user list.</summary>
public sealed class UserListItemViewModel
{
    /// <summary>Gets or sets the Identity user identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the username.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's display name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized role label.</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the account is currently locked.</summary>
    public bool IsLocked { get; set; }

    /// <summary>Gets or sets the UTC instant when the account was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
}

/// <summary>Represents the administrator form used to create an application user.</summary>
public sealed class CreateUserViewModel
{
    /// <summary>Gets or sets the username used for local authentication.</summary>
    [Required(ErrorMessage = "Indique o nome de utilizador."), StringLength(100)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>Gets or sets the name displayed in the application.</summary>
    [Required(ErrorMessage = "Indique o nome a apresentar."), StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's email address.</summary>
    [Required(ErrorMessage = "Indique o email."), EmailAddress(ErrorMessage = "Indique um email válido."), StringLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the initial password.</summary>
    [Required(ErrorMessage = "Indique a palavra-passe inicial."), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Gets or sets the application role assigned to the user.</summary>
    [Required(ErrorMessage = "Selecione o perfil.")]
    public string Role { get; set; } = "User";
}

/// <summary>Represents the administrator form used to edit an existing application user.</summary>
public sealed class EditUserViewModel
{
    /// <summary>Gets or sets the Identity user identifier.</summary>
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the username used for local authentication.</summary>
    [Required(ErrorMessage = "Indique o nome de utilizador."), StringLength(100)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>Gets or sets the name displayed in the application.</summary>
    [Required(ErrorMessage = "Indique o nome a apresentar."), StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's email address.</summary>
    [Required(ErrorMessage = "Indique o email."), EmailAddress(ErrorMessage = "Indique um email válido."), StringLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the application role assigned to the user.</summary>
    [Required(ErrorMessage = "Selecione o perfil.")]
    public string Role { get; set; } = "User";
}

/// <summary>Represents an administrator-initiated password reset for an application user.</summary>
public sealed class ResetUserPasswordViewModel
{
    /// <summary>Gets or sets the Identity user identifier.</summary>
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the username shown for confirmation.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's display name shown for confirmation.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the new password selected by the administrator.</summary>
    [Required(ErrorMessage = "Indique a nova palavra-passe."), DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>Gets or sets the confirmation of the new password.</summary>
    [Required(ErrorMessage = "Confirme a nova palavra-passe."), DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "As palavras-passe não coincidem.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
