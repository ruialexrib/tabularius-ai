namespace TabulariusAI.Web.Models;

/// <summary>
/// Represents a general ledger account read from the SAF-T (PT) master files section.
/// </summary>
public sealed class SaftAccountViewModel
{
    /// <summary>Gets or sets the source account identifier.</summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>Gets or sets the account description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the opening debit balance.</summary>
    public decimal OpeningDebitBalance { get; set; }

    /// <summary>Gets or sets the opening credit balance.</summary>
    public decimal OpeningCreditBalance { get; set; }

    /// <summary>Gets or sets the closing debit balance.</summary>
    public decimal ClosingDebitBalance { get; set; }

    /// <summary>Gets or sets the closing credit balance.</summary>
    public decimal ClosingCreditBalance { get; set; }

    /// <summary>Gets or sets the taxonomy reference when supplied by the source file.</summary>
    public string? TaxonomyReference { get; set; }
}
