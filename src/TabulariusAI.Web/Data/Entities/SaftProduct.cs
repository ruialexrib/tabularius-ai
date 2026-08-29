namespace TabulariusAI.Web.Data.Entities;

/// <summary>
/// Represents a product or service preserved from one exact SAF-T (PT) import.
/// </summary>
public sealed class SaftProduct
{
    /// <summary>Gets or sets the local product row identifier.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the owning SAF-T (PT) import identifier.</summary>
    public int SaftImportId { get; set; }
    /// <summary>Gets or sets the SAF-T product type.</summary>
    public string ProductType { get; set; } = string.Empty;
    /// <summary>Gets or sets the source product code.</summary>
    public string ProductCode { get; set; } = string.Empty;
    /// <summary>Gets or sets the optional source product group.</summary>
    public string? ProductGroup { get; set; }
    /// <summary>Gets or sets the product or service description.</summary>
    public string ProductDescription { get; set; } = string.Empty;
    /// <summary>Gets or sets the product number code used by the source system.</summary>
    public string ProductNumberCode { get; set; } = string.Empty;
    /// <summary>Gets or sets the SAF-T (PT) import that owns this row.</summary>
    public SaftImport SaftImport { get; set; } = null!;
}
