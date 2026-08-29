namespace TabulariusAI.Web.Models;

/// <summary>
/// Represents one product or service parsed from the SAF-T (PT) master files section.
/// </summary>
public sealed class SaftProductViewModel
{
    /// <summary>Gets or sets the SAF-T product type.</summary>
    public string ProductType { get; set; } = string.Empty;
    /// <summary>Gets or sets the source product code.</summary>
    public string ProductCode { get; set; } = string.Empty;
    /// <summary>Gets or sets the optional source product group.</summary>
    public string? ProductGroup { get; set; }
    /// <summary>Gets or sets the product or service description.</summary>
    public string ProductDescription { get; set; } = string.Empty;
    /// <summary>Gets or sets the source product number code.</summary>
    public string ProductNumberCode { get; set; } = string.Empty;
}
