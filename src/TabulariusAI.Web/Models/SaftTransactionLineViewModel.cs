namespace TabulariusAI.Web.Models;

/// <summary>
/// Represents one debit or credit line parsed from a SAF-T (PT) accounting transaction.
/// </summary>
public sealed class SaftTransactionLineViewModel
{
    /// <summary>Gets or sets the source record identifier.</summary>
    public string RecordId { get; set; } = string.Empty;
    /// <summary>Gets or sets the ledger account identifier.</summary>
    public string AccountId { get; set; } = string.Empty;
    /// <summary>Gets or sets the source document identifier when present.</summary>
    public string? SourceDocumentId { get; set; }
    /// <summary>Gets or sets the source system entry date and time when present.</summary>
    public DateTime? SystemEntryDate { get; set; }
    /// <summary>Gets or sets the line description.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Gets or sets D for debit or C for credit.</summary>
    public string Side { get; set; } = string.Empty;
    /// <summary>Gets or sets the monetary amount represented by the line.</summary>
    public decimal Amount { get; set; }
}
