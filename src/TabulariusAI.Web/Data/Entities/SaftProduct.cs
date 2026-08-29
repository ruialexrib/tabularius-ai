namespace TabulariusAI.Web.Data.Entities;

/// <summary>
/// Represents one product or service preserved from a specific SAF-T (PT) import.
/// </summary>
public sealed class SaftProduct
{
    /// <summary>Gets or sets the local identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the owning SAF-T import identifier.</summary>
    public int SaftImportId { get; set; }

    /// <summary>Gets or sets the source product or service code.</summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the product or service type declared in SAF-T (PT).</summary>
    public string ProductType { get; set; } = string.Empty;

    /// <summary>Gets or sets the product group.</summary>
    public string ProductGroup { get; set; } = string.Empty;

    /// <summary>Gets or sets the product or service description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the SAF-T import that owns this source record.</summary>
    public SaftImport SaftImport { get; set; } = null!;
}
