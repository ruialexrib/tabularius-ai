namespace TabulariusAI.Web.Data.Entities;

/// <summary>
/// Represents customer master data preserved from one exact SAF-T (PT) import.
/// </summary>
public sealed class SaftCustomer
{
    /// <summary>Gets or sets the database identifier.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the owning SAF-T import identifier.</summary>
    public int SaftImportId { get; set; }
    /// <summary>Gets or sets the source customer identifier.</summary>
    public string CustomerId { get; set; } = string.Empty;
    /// <summary>Gets or sets the source ledger account identifier.</summary>
    public string AccountId { get; set; } = string.Empty;
    /// <summary>Gets or sets the customer tax identifier.</summary>
    public string TaxId { get; set; } = string.Empty;
    /// <summary>Gets or sets the customer company or person name.</summary>
    public string CompanyName { get; set; } = string.Empty;
    /// <summary>Gets or sets the SAF-T import that owns this customer.</summary>
    public SaftImport SaftImport { get; set; } = null!;
}
