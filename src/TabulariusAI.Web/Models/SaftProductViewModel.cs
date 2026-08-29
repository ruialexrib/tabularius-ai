namespace TabulariusAI.Web.Models;

/// <summary>
/// Represents a product or service parsed from the SAF-T (PT) master files section.
/// </summary>
public sealed class SaftProductViewModel
{
    /// <summary>Gets or sets the source product or service code.</summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the product or service type declared in SAF-T (PT).</summary>
    public string ProductType { get; set; } = string.Empty;

    /// <summary>Gets or sets the product group.</summary>
    public string ProductGroup { get; set; } = string.Empty;

    /// <summary>Gets or sets the product or service description.</summary>
    public string Description { get; set; } = string.Empty;
}
