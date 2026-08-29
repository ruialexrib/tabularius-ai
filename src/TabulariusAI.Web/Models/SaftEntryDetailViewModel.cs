using TabulariusAI.Web.Data.Entities;

namespace TabulariusAI.Web.Models;

/// <summary>Provides source context and persisted lines for one SAF-T (PT) accounting entry.</summary>
public sealed class SaftEntryDetailViewModel
{
    /// <summary>Gets or sets the selected SAF-T (PT) source context.</summary>
    public SaftImportSelectionViewModel Source { get; set; } = null!;
    /// <summary>Gets or sets the accounting transaction being displayed.</summary>
    public SaftTransaction Transaction { get; set; } = null!;
    /// <summary>Gets the deterministic total debit amount represented by the entry lines.</summary>
    public decimal TotalDebit => Transaction.Lines.Where(line => line.Side == "D").Sum(line => line.Amount);
    /// <summary>Gets the deterministic total credit amount represented by the entry lines.</summary>
    public decimal TotalCredit => Transaction.Lines.Where(line => line.Side == "C").Sum(line => line.Amount);
}
