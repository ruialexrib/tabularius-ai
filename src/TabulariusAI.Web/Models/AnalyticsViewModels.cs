namespace TabulariusAI.Web.Models;

/// <summary>Represents the deterministic overview for one selected SAF-T source.</summary>
public sealed class AnalyticsOverviewViewModel
{
    /// <summary>Gets or sets the selected SAF-T source context.</summary>
    public SaftImportSelectionViewModel Source { get; set; } = null!;
    /// <summary>Gets or sets the number of accounting transactions.</summary>
    public int TransactionCount { get; set; }
    /// <summary>Gets or sets total debit movements.</summary>
    public decimal TotalDebit { get; set; }
    /// <summary>Gets or sets total credit movements.</summary>
    public decimal TotalCredit { get; set; }
    /// <summary>Gets or sets the number of accounts with movements.</summary>
    public int ActiveAccountCount { get; set; }
    /// <summary>Gets or sets the number of detected deterministic anomalies.</summary>
    public int AnomalyCount { get; set; }
    /// <summary>Gets or sets monthly accounting activity.</summary>
    public IReadOnlyList<AnalyticsMonthlyRow> Monthly { get; set; } = [];
    /// <summary>Gets or sets the accounts with the largest movement volume.</summary>
    public IReadOnlyList<AccountAnalysisRow> TopAccounts { get; set; } = [];
}

/// <summary>Represents debit and credit activity for one month.</summary>
public sealed record AnalyticsMonthlyRow(int Month, decimal Debit, decimal Credit);

/// <summary>Represents deterministic movement analysis for one account.</summary>
public sealed record AccountAnalysisRow(string AccountId, string Description, decimal Debit, decimal Credit, decimal NetMovement, int LineCount);

/// <summary>Represents the account-analysis workspace.</summary>
public sealed class AccountAnalysisViewModel
{
    /// <summary>Gets or sets the selected SAF-T source context.</summary>
    public SaftImportSelectionViewModel Source { get; set; } = null!;
    /// <summary>Gets or sets the optional search term.</summary>
    public string? Search { get; set; }
    /// <summary>Gets or sets the analysed accounts.</summary>
    public IReadOnlyList<AccountAnalysisRow> Rows { get; set; } = [];
}

/// <summary>Represents a deterministic anomaly detected in accounting data.</summary>
public sealed record AccountingAnomaly(string Severity, string Type, string Reference, string Description, decimal? Difference, int? TransactionId);

/// <summary>Represents the anomaly-analysis workspace.</summary>
public sealed class AnomaliesViewModel
{
    /// <summary>Gets or sets the selected SAF-T source context.</summary>
    public SaftImportSelectionViewModel Source { get; set; } = null!;
    /// <summary>Gets or sets the deterministic findings.</summary>
    public IReadOnlyList<AccountingAnomaly> Findings { get; set; } = [];
}
