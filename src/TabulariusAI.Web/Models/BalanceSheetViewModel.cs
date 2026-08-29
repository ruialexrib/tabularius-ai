namespace TabulariusAI.Web.Models;

public sealed class BalanceSheetViewModel
{
    public SaftImportSelectionViewModel Source { get; set; } = null!;
    public IReadOnlyList<BalanceSheetRowViewModel> Assets { get; set; } = [];
    public IReadOnlyList<BalanceSheetRowViewModel> Equity { get; set; } = [];
    public IReadOnlyList<BalanceSheetRowViewModel> Liabilities { get; set; } = [];
    public IReadOnlyList<BalanceSheetRowViewModel> Unclassified { get; set; } = [];
    public decimal TotalAssets => Assets.Sum(item => item.Amount);
    public decimal TotalEquity => Equity.Sum(item => item.Amount);
    public decimal TotalLiabilities => Liabilities.Sum(item => item.Amount);
    public decimal Difference => TotalAssets - TotalEquity - TotalLiabilities;
    public bool IsBalanced => Difference == 0m && Unclassified.Count == 0;
}

public sealed class BalanceSheetRowViewModel
{
    public string AccountId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Classification { get; set; } = string.Empty;
}
