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
public sealed record AnalyticsMonthlyRow(int Month, decimal Debit, decimal Credit);
public sealed record AccountAnalysisRow(string AccountId, string Description, decimal Debit, decimal Credit, decimal NetMovement, int LineCount);
public sealed class AccountAnalysisViewModel{public SaftImportSelectionViewModel Source{get;set;}=null!;public string? Search{get;set;}public IReadOnlyList<AccountAnalysisRow> Rows{get;set;}=[];}
public sealed record AccountMovementRow(int TransactionLocalId,string TransactionId,DateOnly Date,string JournalId,string Description,string Side,decimal Amount,string? SourceDocumentId);
public sealed record CounterpartAccountRow(string AccountId,string Description,int TransactionCount,decimal MovementVolume);
public sealed class AccountInvestigationViewModel{public SaftImportSelectionViewModel Source{get;set;}=null!;public string AccountId{get;set;}=string.Empty;public string Description{get;set;}=string.Empty;public decimal OpeningDebit{get;set;}public decimal OpeningCredit{get;set;}public decimal ClosingDebit{get;set;}public decimal ClosingCredit{get;set;}public decimal Debit{get;set;}public decimal Credit{get;set;}public IReadOnlyList<AnalyticsMonthlyRow> Monthly{get;set;}=[];public IReadOnlyList<AccountMovementRow> Movements{get;set;}=[];public IReadOnlyList<CounterpartAccountRow> Counterparts{get;set;}=[];}
public sealed record AccountingAnomaly(string RuleId,string Severity,string Type,string Reference,string Description,decimal? Difference,int? TransactionId);
public sealed record AccountingAnomalyRuleCheck(string RuleId,string Name,string Description,string Severity,int FindingCount){public bool Passed=>FindingCount==0;}
public sealed class AnomaliesViewModel{public SaftImportSelectionViewModel Source{get;set;}=null!;public IReadOnlyList<AccountingAnomaly> Findings{get;set;}=[];public IReadOnlyList<AccountingAnomalyRuleCheck> Checks{get;set;}=[];}

/// <summary>Aggregates the net fiscal effect of sales documents for one VAT rate.</summary>
public sealed record VatRateSummaryRow(decimal? TaxPercentage,decimal TaxableBase,decimal VatAmount,decimal GrossTotal,int DocumentCount);
/// <summary>Shows the signed fiscal effect of one sales document and VAT rate.</summary>
public sealed record VatDocumentRow(string InvoiceNo,DateOnly Date,string InvoiceType,string? CustomerId,decimal? TaxPercentage,decimal TaxableBase,decimal VatAmount,decimal GrossTotal);
/// <summary>Represents deterministic VAT analysis from persisted SAF-T sales documents.</summary>
public sealed class VatAnalysisViewModel
{
    public SaftImportSelectionViewModel Source{get;set;}=null!;
    public decimal TotalTaxableBase{get;set;}
    public decimal TotalVat{get;set;}
    public decimal TotalGross{get;set;}
    public IReadOnlyList<VatRateSummaryRow> Rates{get;set;}=[];
    public IReadOnlyList<VatDocumentRow> Documents{get;set;}=[];
}
