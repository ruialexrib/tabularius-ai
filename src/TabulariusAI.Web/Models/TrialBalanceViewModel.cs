namespace TabulariusAI.Web.Models;

/// <summary>Represents the deterministic trial balance for one selected SAF-T (PT) source.</summary>
public sealed class TrialBalanceViewModel
{
    public SaftImportSelectionViewModel Source { get; set; } = null!;
    public IReadOnlyList<TrialBalanceRowViewModel> Rows { get; set; } = [];
    public string? Search { get; set; }
    public bool IncludeZeroAccounts { get; set; }
    public decimal TotalOpeningDebit => Rows.Where(item => !item.IsAggregateAccount).Sum(item => item.OpeningDebit);
    public decimal TotalOpeningCredit => Rows.Where(item => !item.IsAggregateAccount).Sum(item => item.OpeningCredit);
    public decimal TotalDebitMovements => Rows.Where(item => !item.IsAggregateAccount).Sum(item => item.DebitMovements);
    public decimal TotalCreditMovements => Rows.Where(item => !item.IsAggregateAccount).Sum(item => item.CreditMovements);
    public decimal TotalClosingDebit => Rows.Where(item => !item.IsAggregateAccount).Sum(item => item.ClosingDebit);
    public decimal TotalClosingCredit => Rows.Where(item => !item.IsAggregateAccount).Sum(item => item.ClosingCredit);
    public decimal MovementDifference => TotalDebitMovements - TotalCreditMovements;
}

/// <summary>Represents one account line in a deterministic trial balance.</summary>
public sealed class TrialBalanceRowViewModel
{
    public string AccountId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal DebitMovements { get; set; }
    public decimal CreditMovements { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
    public decimal ReportedClosingDebit { get; set; }
    public decimal ReportedClosingCredit { get; set; }
    /// <summary>Gets or sets whether this account has descendant accounts in the imported chart of accounts.</summary>
    public bool IsAggregateAccount { get; set; }
    /// <summary>Gets whether the calculated closing balance differs from the SAF-T closing balance for a movement account.</summary>
    public bool HasClosingDifference => !IsAggregateAccount && (ClosingDebit != ReportedClosingDebit || ClosingCredit != ReportedClosingCredit);
}
