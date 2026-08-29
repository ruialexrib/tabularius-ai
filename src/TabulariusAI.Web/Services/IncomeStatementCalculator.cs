using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Services;

/// <summary>Calculates a deterministic income statement from SNC class 6 and 7 movement accounts.</summary>
public static class IncomeStatementCalculator
{
    public static IncomeStatementViewModel Calculate(IEnumerable<SaftAccount> accounts, IEnumerable<SaftTransactionLine> lines)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(lines);

        var accountList = accounts.ToList();
        var accountIds = accountList.Select(item => item.AccountId).ToArray();
        var movements = lines
            .GroupBy(item => item.AccountId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => new
                {
                    Debit = group.Where(item => string.Equals(item.Side, "D", StringComparison.OrdinalIgnoreCase)).Sum(item => item.Amount),
                    Credit = group.Where(item => string.Equals(item.Side, "C", StringComparison.OrdinalIgnoreCase)).Sum(item => item.Amount)
                }, StringComparer.OrdinalIgnoreCase);

        var movementAccounts = accountList.Where(account =>
            !accountIds.Any(candidate => candidate.Length > account.AccountId.Length && candidate.StartsWith(account.AccountId, StringComparison.OrdinalIgnoreCase)));

        var expenses = new List<IncomeStatementRowViewModel>();
        var income = new List<IncomeStatementRowViewModel>();

        foreach (var account in movementAccounts)
        {
            if (!movements.TryGetValue(account.AccountId, out var movement)) continue;
            if (account.AccountId.StartsWith("6", StringComparison.OrdinalIgnoreCase))
            {
                var amount = movement.Debit - movement.Credit;
                if (amount != 0m) expenses.Add(Row(account, movement.Debit, movement.Credit, amount));
            }
            else if (account.AccountId.StartsWith("7", StringComparison.OrdinalIgnoreCase))
            {
                var amount = movement.Credit - movement.Debit;
                if (amount != 0m) income.Add(Row(account, movement.Debit, movement.Credit, amount));
            }
        }

        return new IncomeStatementViewModel
        {
            Expenses = expenses.OrderBy(item => item.AccountId, StringComparer.OrdinalIgnoreCase).ToList(),
            Income = income.OrderBy(item => item.AccountId, StringComparer.OrdinalIgnoreCase).ToList()
        };
    }

    private static IncomeStatementRowViewModel Row(SaftAccount account, decimal debit, decimal credit, decimal amount) => new()
    {
        AccountId = account.AccountId,
        Description = account.Description,
        DebitMovements = debit,
        CreditMovements = credit,
        Amount = amount
    };
}
