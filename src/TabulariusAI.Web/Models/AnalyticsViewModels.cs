namespace TabulariusAI.Web.Models;

/// <summary>Represents the deterministic overview for one selected SAF-T source.</summary>
public sealed class AnalyticsOverviewViewModel
{
    public SaftImportSelectionViewModel Source { get; set; } = null!;
    public int TransactionCount { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public int ActiveAccountCount { get; set; }
    public int AnomalyCount { get; set; }
    public IReadOnlyList<AnalyticsMonthlyRow> Monthly { get; set; } = [];
    public IReadOnlyList<AccountAnalysisRow> TopAccounts { get; set; } = [];
}

/// <summary>Represents debit and credit activity for one month.</summary>
public sealed record AnalyticsMonthlyRow(int Month, decimal Debit, decimal Credit);

/// <summary>Represents deterministic movement analysis for one account.</summary>
public sealed record AccountAnalysisRow(string AccountId, string Description, decimal Debit, decimal Credit, decimal NetMovement, int LineCount);

/// <summary>Represents the account-analysis workspace.</summary>
public sealed class AccountAnalysisViewModel
{
    public SaftImportSelectionViewModel Source { get; set; } = null!;
    public string? Search { get; set; }
    public IReadOnlyList<AccountAnalysisRow> Rows { get; set; } = [];
}

/// <summary>Represents one accounting movement in an account investigation.</summary>
public sealed record AccountMovementRow(int TransactionLocalId, string TransactionId, DateOnly Date, string JournalId, string Description, string Side, decimal Amount, string? SourceDocumentId);

/// <summary>Represents a counterpart account observed in transactions containing the investigated account.</summary>
public sealed record CounterpartAccountRow(string AccountId, string Description, int TransactionCount, decimal MovementVolume);

/// <summary>Represents the deterministic account investigation workspace.</summary>
public sealed class AccountInvestigationViewModel
{
    public SaftImportSelectionViewModel Source { get; set; } = null!;
    public string AccountId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public IReadOnlyList<AnalyticsMonthlyRow> Monthly { get; set; } = [];
    public IReadOnlyList<AccountMovementRow> Movements { get; set; } = [];
    public IReadOnlyList<CounterpartAccountRow> Counterparts { get; set; } = [];
}

/// <summary>Represents a deterministic anomaly detected in accounting data.</summary>
public sealed record AccountingAnomaly(string RuleId, string Severity, string Type, string Reference, string Description, decimal? Difference, int? TransactionId);

/// <summary>Represents the anomaly-analysis workspace.</summary>
public sealed class AnomaliesViewModel
{
    public SaftImportSelectionViewModel Source { get; set; } = null!;
    public IReadOnlyList<AccountingAnomaly> Findings { get; set; } = [];
}
