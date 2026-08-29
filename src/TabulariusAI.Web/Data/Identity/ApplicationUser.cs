using Microsoft.AspNetCore.Identity;

namespace TabulariusAI.Web.Data.Identity;

/// <summary>Represents an authenticated Tabularius AI user.</summary>
public sealed class ApplicationUser : IdentityUser
{
    /// <summary>Gets or sets the display name shown in the application.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC instant when the account was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
