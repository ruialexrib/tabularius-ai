namespace TabulariusAI.Web.Data.Entities;

/// <summary>Represents one accounting transaction imported from SAF-T (PT) GeneralLedgerEntries.</summary>
public sealed class SaftTransaction
{
    /// <summary>Gets or sets the local transaction identifier.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the owning SAF-T import identifier.</summary>
    public int SaftImportId { get; set; }
    /// <summary>Gets or sets the journal identifier.</summary>
    public string JournalId { get; set; } = string.Empty;
    /// <summary>Gets or sets the journal description.</summary>
    public string JournalDescription { get; set; } = string.Empty;
    /// <summary>Gets or sets the source transaction identifier.</summary>
    public string TransactionId { get; set; } = string.Empty;
    /// <summary>Gets or sets the accounting period.</summary>
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
    /// <summary>Gets or sets the customer identifier when present.</summary>
    public string? CustomerId { get; set; }
    /// <summary>Gets or sets the supplier identifier when present.</summary>
    public string? SupplierId { get; set; }
    /// <summary>Gets or sets the owning SAF-T import.</summary>
    public SaftImport SaftImport { get; set; } = null!;
    /// <summary>Gets the debit and credit lines that belong to this transaction.</summary>
    public ICollection<SaftTransactionLine> Lines { get; } = new List<SaftTransactionLine>();
}
