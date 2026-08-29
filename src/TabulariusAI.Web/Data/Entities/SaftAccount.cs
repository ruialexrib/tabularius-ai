namespace TabulariusAI.Web.Data.Entities;

/// <summary>
/// Represents a general ledger account imported from a specific SAF-T (PT) dataset.
/// </summary>
public sealed class SaftAccount
{
    /// <summary>Gets or sets the local account record identifier.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the owning SAF-T (PT) import identifier.</summary>
    public int SaftImportId { get; set; }
    /// <summary>Gets or sets the source AccountID value.</summary>
    public string AccountId { get; set; } = string.Empty;
    /// <summary>Gets or sets the source account description.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Gets or sets the source opening debit balance.</summary>
    public decimal OpeningDebitBalance { get; set; }
    /// <summary>Gets or sets the source opening credit balance.</summary>
    public decimal OpeningCreditBalance { get; set; }
    /// <summary>Gets or sets the source closing debit balance.</summary>
    public decimal ClosingDebitBalance { get; set; }
    /// <summary>Gets or sets the source closing credit balance.</summary>
    public decimal ClosingCreditBalance { get; set; }
    /// <summary>Gets or sets the optional source taxonomy reference.</summary>
    public string? TaxonomyReference { get; set; }
    /// <summary>Gets or sets the SAF-T (PT) import that owns this account record.</summary>
    public SaftImport SaftImport { get; set; } = null!;
}
