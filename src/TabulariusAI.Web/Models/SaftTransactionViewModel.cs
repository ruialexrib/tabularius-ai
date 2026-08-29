namespace TabulariusAI.Web.Models;

/// <summary>
/// Represents one accounting transaction parsed from SAF-T (PT) GeneralLedgerEntries.
/// </summary>
public sealed class SaftTransactionViewModel
{
    /// <summary>Gets or sets the journal identifier that contains the transaction.</summary>
    public string JournalId { get; set; } = string.Empty;
    /// <summary>Gets or sets the journal description.</summary>
    public string JournalDescription { get; set; } = string.Empty;
    /// <summary>Gets or sets the source transaction identifier.</summary>
    public string TransactionId { get; set; } = string.Empty;
    /// <summary>Gets or sets the accounting period number.</summary>
    public int Period { get; set; }
    /// <summary>Gets or sets the transaction date.</summary>
    public DateOnly TransactionDate { get; set; }
    /// <summary>Gets or sets the source user or system identifier.</summary>
    public string SourceId { get; set; } = string.Empty;
    /// <summary>Gets or sets the transaction description.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Gets or sets the archival document number when present.</summary>
    public string? DocArchivalNumber { get; set; }
    /// <summary>Gets or sets the SAF-T transaction type.</summary>
    public string TransactionType { get; set; } = string.Empty;
    /// <summary>Gets or sets the general-ledger posting date.</summary>
    public DateOnly GlPostingDate { get; set; }
    /// <summary>Gets or sets the customer identifier when the transaction references a customer.</summary>
    public string? CustomerId { get; set; }
    /// <summary>Gets or sets the supplier identifier when the transaction references a supplier.</summary>
    public string? SupplierId { get; set; }
    /// <summary>Gets the debit and credit lines belonging to this transaction.</summary>
    public IList<SaftTransactionLineViewModel> Lines { get; } = new List<SaftTransactionLineViewModel>();
    /// <summary>Gets the total debit amount calculated from the transaction lines.</summary>
    public decimal TotalDebit => Lines.Where(line => line.Side == "D").Sum(line => line.Amount);
    /// <summary>Gets the total credit amount calculated from the transaction lines.</summary>
    public decimal TotalCredit => Lines.Where(line => line.Side == "C").Sum(line => line.Amount);
}
