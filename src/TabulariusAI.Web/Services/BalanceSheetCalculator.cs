using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Services;

/// <summary>Builds a synthetic balance sheet from closing balances of movement accounts.</summary>
public static class BalanceSheetCalculator
{
    public static BalanceSheetViewModel Calculate(IEnumerable<SaftAccount> accounts)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        var list = accounts.ToList();
        var ids = list.Select(item => item.AccountId).ToArray();
        var model = new BalanceSheetViewModel();
        var assets = new List<BalanceSheetRowViewModel>(); var equity = new List<BalanceSheetRowViewModel>(); var liabilities = new List<BalanceSheetRowViewModel>(); var unclassified = new List<BalanceSheetRowViewModel>();
        foreach (var account in list.Where(account => !ids.Any(candidate => candidate.Length > account.AccountId.Length && candidate.StartsWith(account.AccountId, StringComparison.OrdinalIgnoreCase))))
        {
            var net = account.ClosingDebitBalance - account.ClosingCreditBalance;
            if (net == 0m) continue;
            var target = Classify(account.AccountId, net);
            var row = new BalanceSheetRowViewModel { AccountId = account.AccountId, Description = account.Description, Amount = Math.Abs(net), Classification = target };
            switch (target) { case "Ativo": assets.Add(row); break; case "Capital próprio": equity.Add(row); break; case "Passivo": liabilities.Add(row); break; default: unclassified.Add(row); break; }
        }
        return new BalanceSheetViewModel { Assets = Sort(assets), Equity = Sort(equity), Liabilities = Sort(liabilities), Unclassified = Sort(unclassified) };
    }

    private static string Classify(string accountId, decimal net)
    {
        if (accountId.StartsWith("5", StringComparison.OrdinalIgnoreCase)) return "Capital próprio";
        if (accountId.StartsWith("3", StringComparison.OrdinalIgnoreCase) || accountId.StartsWith("4", StringComparison.OrdinalIgnoreCase) || accountId.StartsWith("1", StringComparison.OrdinalIgnoreCase)) return net >= 0m ? "Ativo" : "Passivo";
        if (accountId.StartsWith("21", StringComparison.OrdinalIgnoreCase) || accountId.StartsWith("23", StringComparison.OrdinalIgnoreCase)) return net >= 0m ? "Ativo" : "Passivo";
        if (accountId.StartsWith("22", StringComparison.OrdinalIgnoreCase) || accountId.StartsWith("25", StringComparison.OrdinalIgnoreCase)) return net <= 0m ? "Passivo" : "Ativo";
        if (accountId.StartsWith("24", StringComparison.OrdinalIgnoreCase) || accountId.StartsWith("26", StringComparison.OrdinalIgnoreCase) || accountId.StartsWith("27", StringComparison.OrdinalIgnoreCase) || accountId.StartsWith("28", StringComparison.OrdinalIgnoreCase)) return net >= 0m ? "Ativo" : "Passivo";
        return "Não classificada";
    }

    private static IReadOnlyList<BalanceSheetRowViewModel> Sort(List<BalanceSheetRowViewModel> rows) => rows.OrderBy(item => item.AccountId, StringComparer.OrdinalIgnoreCase).ToList();
}
