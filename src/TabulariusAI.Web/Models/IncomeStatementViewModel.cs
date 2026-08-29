namespace TabulariusAI.Web.Models;

public sealed class IncomeStatementViewModel
{
    public SaftImportSelectionViewModel Source { get; set; } = null!;
    public IReadOnlyList<IncomeStatementRowViewModel> Expenses { get; set; } = [];
    public IReadOnlyList<IncomeStatementRowViewModel> Income { get; set; } = [];
    public decimal TotalExpenses => Expenses.Sum(item => item.Amount);
    public decimal TotalIncome => Income.Sum(item => item.Amount);
    public decimal NetResult => TotalIncome - TotalExpenses;
}

public sealed class IncomeStatementRowViewModel
{
    public string AccountId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DebitMovements { get; set; }
    public decimal CreditMovements { get; set; }
    public decimal Amount { get; set; }
}
