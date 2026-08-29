namespace TabulariusAI.Web.Models;

/// <summary>
/// Represents diagnostic information that can be safely shown on the application error page.
/// </summary>
public sealed class ErrorViewModel
{
    /// <summary>
    /// Gets or sets the request identifier used to correlate the user-visible error with diagnostic logs.
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether a request identifier is available for display.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrWhiteSpace(RequestId);
}
