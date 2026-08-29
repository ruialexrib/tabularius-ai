namespace TabulariusAI.Web.Data.Identity;

/// <summary>Defines the application roles used by Tabularius AI authorization.</summary>
public static class ApplicationRoles
{
    /// <summary>Gets the administrator role name.</summary>
    public const string Administrator = "Administrator";

    /// <summary>Gets the standard user role name.</summary>
    public const string User = "User";

    /// <summary>Gets all roles that must exist in the identity store.</summary>
    public static IReadOnlyCollection<string> All { get; } = [Administrator, User];
}
