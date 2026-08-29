namespace TabulariusAI.Web.Models;

/// <summary>
/// Represents the SAF-T header information displayed after a successful file analysis.
/// </summary>
public sealed class SaftHeaderViewModel
{
    /// <summary>
    /// Gets or sets the SAF-T namespace or schema identifier detected in the document.
    /// </summary>
    public string SaftVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tax registration number of the company represented by the SAF-T file.
    /// </summary>
    public string TaxRegistrationNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the company name declared in the SAF-T header.
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fiscal year declared in the SAF-T header.
    /// </summary>
    public string FiscalYear { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start date of the accounting period represented by the file.
    /// </summary>
    public string StartDate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the end date of the accounting period represented by the file.
    /// </summary>
    public string EndDate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product identifier of the software that generated the SAF-T file.
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product version of the software that generated the SAF-T file.
    /// </summary>
    public string ProductVersion { get; set; } = string.Empty;
}
