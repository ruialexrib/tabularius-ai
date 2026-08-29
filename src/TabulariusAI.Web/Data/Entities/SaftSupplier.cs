namespace TabulariusAI.Web.Data.Entities;

/// <summary>
/// Represents supplier master data preserved from one exact SAF-T (PT) import.
/// </summary>
public sealed class SaftSupplier
{
    /// <summary>Gets or sets the database identifier.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the owning SAF-T import identifier.</summary>
    public int SaftImportId { get; set; }
    /// <summary>Gets or sets the source supplier identifier.</summary>
    public string SupplierId { get; set; } = string.Empty;
    /// <summary>Gets or sets the source ledger account identifier.</summary>
    public string AccountId { get; set; } = string.Empty;
    /// <summary>Gets or sets the supplier tax identifier.</summary>
    public string TaxId { get; set; } = string.Empty;
    /// <summary>Gets or sets the supplier company or person name.</summary>
    public string CompanyName { get; set; } = string.Empty;
    /// <summary>Gets or sets the SAF-T import that owns this supplier.</summary>
    public SaftImport SaftImport { get; set; } = null!;
}
