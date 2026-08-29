namespace TabulariusAI.Web.Models;

/// <summary>Represents the deterministic trial balance for one selected SAF-T (PT) source.</summary>
public sealed class TrialBalanceViewModel
{
    /// <summary>Gets or sets the selected SAF-T (PT) source context.</summary>
    public SaftImportSelectionViewModel Source { get; set; } = null!;
    /// <summary>Gets or sets the account rows included in the trial balance.</summary>
    public IReadOnlyList<TrialBalanceRowViewModel> Rows { get; set; } = [];
    /// <summary>Gets or sets the optional account search term.</summary>
    public string? Search { get; set; }
    /// <summary>Gets or sets whether accounts without opening balances, movements or closing balances are included.</summary>
    public bool IncludeZeroAccounts { get; set; }
    /// <summary>Gets the total opening debit balance.</summary>
    public decimal TotalOpeningDebit => Rows.Sum(item => item.OpeningDebit);
    /// <summary>Gets the total opening credit balance.</summary>
    public decimal TotalOpeningCredit => Rows.Sum(item => item.OpeningCredit);
    /// <summary>Gets the total debit movements.</summary>
    public decimal TotalDebitMovements => Rows.Sum(item => item.DebitMovements);
    /// <summary>Gets the total credit movements.</summary>
    public decimal TotalCreditMovements => Rows.Sum(item => item.CreditMovements);
    /// <summary>Gets the total calculated closing debit balance.</summary>
    public decimal TotalClosingDebit => Rows.Sum(item => item.ClosingDebit);
    /// <summary>Gets the total calculated closing credit balance.</summary>
    public decimal TotalClosingCredit => Rows.Sum(item => item.ClosingCredit);
    /// <summary>Gets the difference between total debit and credit movements.</summary>
    public decimal MovementDifference => TotalDebitMovements - TotalCreditMovements;
}

/// <summary>Represents one account line in a deterministic trial balance.</summary>
public sealed class TrialBalanceRowViewModel
{
    /// <summary>Gets or sets the SAF-T account identifier.</summary>
    public string AccountId { get; set; } = string.Empty;
    /// <summary>Gets or sets the account description.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Gets or sets the opening debit balance from the selected SAF-T source.</summary>
    public decimal OpeningDebit { get; set; }
    /// <summary>Gets or sets the opening credit balance from the selected SAF-T source.</summary>
    public decimal OpeningCredit { get; set; }
    /// <summary>Gets or sets debit movements calculated from accounting transaction lines.</summary>
    public decimal DebitMovements { get; set; }
    /// <summary>Gets or sets credit movements calculated from accounting transaction lines.</summary>
    public decimal CreditMovements { get; set; }
    /// <summary>Gets or sets the calculated closing debit balance.</summary>
    public decimal ClosingDebit { get; set; }
    /// <summary>Gets or sets the calculated closing credit balance.</summary>
    public decimal ClosingCredit { get; set; }
    /// <summary>Gets or sets the closing debit balance reported by the SAF-T account master file.</summary>
    public decimal ReportedClosingDebit { get; set; }
    /// <summary>Gets or sets the closing credit balance reported by the SAF-T account master file.</summary>
    public decimal ReportedClosingCredit { get; set; }
    /// <summary>Gets whether the calculated closing balance differs from the source account closing balance.</summary>
    public bool HasClosingDifference => ClosingDebit != ReportedClosingDebit || ClosingCredit != ReportedClosingCredit;
}
