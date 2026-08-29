namespace TabulariusAI.Web.Models;

/// <summary>
/// Provides application metadata that is displayed by the shared user interface.
/// </summary>
/// <param name="Version">The current application version.</param>
/// <param name="Description">The application description.</param>
public sealed record ApplicationInfo(string Version, string Description);
