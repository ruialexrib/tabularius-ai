using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Services;

/// <summary>Calculates trial-balance rows deterministically from SAF-T accounts and accounting lines.</summary>
public static class TrialBalanceCalculator
{
    /// <summary>Calculates trial-balance rows for the supplied accounts and transaction lines.</summary>
    /// <param name="accounts">Accounts from one SAF-T (PT) import.</param>
    /// <param name="lines">Accounting lines from the same SAF-T (PT) import.</param>
    /// <returns>Trial-balance rows ordered by account identifier.</returns>
    public static IReadOnlyList<TrialBalanceRowViewModel> Calculate(IEnumerable<SaftAccount> accounts, IEnumerable<SaftTransactionLine> lines)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(lines);

        var movements = lines
            .GroupBy(item => item.AccountId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => new
                {
                    Debit = group.Where(item => string.Equals(item.Side, "D", StringComparison.OrdinalIgnoreCase)).Sum(item => item.Amount),
                    Credit = group.Where(item => string.Equals(item.Side, "C", StringComparison.OrdinalIgnoreCase)).Sum(item => item.Amount)
                },
                StringComparer.OrdinalIgnoreCase);

        return accounts
            .Select(account =>
            {
                movements.TryGetValue(account.AccountId, out var movement);
                var debitMovements = movement?.Debit ?? 0m;
                var creditMovements = movement?.Credit ?? 0m;
                var netClosing = account.OpeningDebitBalance - account.OpeningCreditBalance + debitMovements - creditMovements;
                return new TrialBalanceRowViewModel
                {
                    AccountId = account.AccountId,
                    Description = account.Description,
                    OpeningDebit = account.OpeningDebitBalance,
                    OpeningCredit = account.OpeningCreditBalance,
                    DebitMovements = debitMovements,
                    CreditMovements = creditMovements,
                    ClosingDebit = netClosing > 0m ? netClosing : 0m,
                    ClosingCredit = netClosing < 0m ? -netClosing : 0m,
                    ReportedClosingDebit = account.ClosingDebitBalance,
                    ReportedClosingCredit = account.ClosingCreditBalance
                };
            })
            .OrderBy(item => item.AccountId, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
