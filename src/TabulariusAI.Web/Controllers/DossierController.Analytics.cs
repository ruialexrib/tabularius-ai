using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Controllers;

/// <summary>Provides deterministic analytical views for the selected accounting dossier source.</summary>
public sealed partial class DossierController
{
    /// <summary>Displays the deterministic analytical overview.</summary>
    public async Task<IActionResult> Analytics(int id, int? importId, CancellationToken ct = default)
    {
        var source = await LoadSourceAsync(id, importId, ct);
        if (source is null) return NotFound();
        var selectedId = source.SelectedImport.Id;
        var transactions = await dbContext.SaftTransactions.AsNoTracking().Where(x => x.SaftImportId == selectedId).Select(x => new { x.Id, x.TransactionDate }).ToListAsync(ct);
        var lines = await dbContext.SaftTransactionLines.AsNoTracking().Where(x => x.SaftTransaction.SaftImportId == selectedId).Select(x => new { x.SaftTransactionId, x.AccountId, x.Side, x.Amount }).ToListAsync(ct);
        var accounts = await dbContext.SaftAccounts.AsNoTracking().Where(x => x.SaftImportId == selectedId).Select(x => new { x.AccountId, x.Description }).ToListAsync(ct);
        var accountNames = accounts.ToDictionary(x => x.AccountId, x => x.Description, StringComparer.OrdinalIgnoreCase);
        var transactionDates = transactions.ToDictionary(x => x.Id, x => x.TransactionDate);
        var topAccounts = lines.GroupBy(x => x.AccountId).Select(g => new AccountAnalysisRow(g.Key, accountNames.GetValueOrDefault(g.Key) ?? "Conta não identificada no plano", g.Where(x => x.Side == "D").Sum(x => x.Amount), g.Where(x => x.Side == "C").Sum(x => x.Amount), g.Where(x => x.Side == "D").Sum(x => x.Amount) - g.Where(x => x.Side == "C").Sum(x => x.Amount), g.Count())).OrderByDescending(x => x.Debit + x.Credit).Take(8).ToList();
        var monthly = lines.Where(x => transactionDates.ContainsKey(x.SaftTransactionId)).GroupBy(x => transactionDates[x.SaftTransactionId].Month).Select(g => new AnalyticsMonthlyRow(g.Key, g.Where(x => x.Side == "D").Sum(x => x.Amount), g.Where(x => x.Side == "C").Sum(x => x.Amount))).OrderBy(x => x.Month).ToList();
        var anomalyCount = CountUnbalancedTransactions(lines) + lines.Count(x => x.Amount < 0) + lines.Count(x => x.Side != "D" && x.Side != "C");
        return View(new AnalyticsOverviewViewModel { Source = source, TransactionCount = transactions.Count, TotalDebit = lines.Where(x => x.Side == "D").Sum(x => x.Amount), TotalCredit = lines.Where(x => x.Side == "C").Sum(x => x.Amount), ActiveAccountCount = lines.Select(x => x.AccountId).Distinct(StringComparer.OrdinalIgnoreCase).Count(), AnomalyCount = anomalyCount, Monthly = monthly, TopAccounts = topAccounts });
    }

    /// <summary>Displays deterministic accounting anomalies with traceable transaction references.</summary>
    public async Task<IActionResult> Anomalies(int id, int? importId, CancellationToken ct = default)
    {
        var source = await LoadSourceAsync(id, importId, ct);
        if (source is null) return NotFound();
        var selectedId = source.SelectedImport.Id;
        var transactions = await dbContext.SaftTransactions.AsNoTracking().Where(x => x.SaftImportId == selectedId).Select(x => new { x.Id, x.TransactionId, x.Description }).ToListAsync(ct);
        var lines = await dbContext.SaftTransactionLines.AsNoTracking().Where(x => x.SaftTransaction.SaftImportId == selectedId).Select(x => new { x.Id, x.SaftTransactionId, x.RecordId, x.AccountId, x.Side, x.Amount }).ToListAsync(ct);
        var findings = new List<AccountingAnomaly>();
        foreach (var transaction in transactions)
        {
            var txLines = lines.Where(x => x.SaftTransactionId == transaction.Id).ToList();
            var debit = txLines.Where(x => x.Side == "D").Sum(x => x.Amount);
            var credit = txLines.Where(x => x.Side == "C").Sum(x => x.Amount);
            var difference = debit - credit;
            if (difference != 0) findings.Add(new("Alta", "Lançamento desequilibrado", transaction.TransactionId, $"Débitos e créditos do lançamento não coincidem. {transaction.Description}", difference, transaction.Id));
        }
        findings.AddRange(lines.Where(x => x.Amount < 0).Select(x => new AccountingAnomaly("Média", "Montante negativo", x.RecordId, $"A linha da conta {x.AccountId} apresenta um montante negativo.", x.Amount, x.SaftTransactionId)));
        findings.AddRange(lines.Where(x => x.Side != "D" && x.Side != "C").Select(x => new AccountingAnomaly("Alta", "Natureza inválida", x.RecordId, $"A linha da conta {x.AccountId} não está identificada como débito ou crédito.", null, x.SaftTransactionId)));
        var duplicateTransactions = transactions.GroupBy(x => x.TransactionId, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1);
        findings.AddRange(duplicateTransactions.Select(g => new AccountingAnomaly("Média", "Identificador duplicado", g.Key, $"O identificador de lançamento ocorre {g.Count()} vezes nesta fonte SAF-T (PT).", null, g.First().Id)));
        return View(new AnomaliesViewModel { Source = source, Findings = findings.OrderBy(x => x.Severity == "Alta" ? 0 : 1).ThenBy(x => x.Type).ToList() });
    }

    /// <summary>Displays deterministic debit, credit and net movement analysis by ledger account.</summary>
    public async Task<IActionResult> AccountAnalysis(int id, int? importId, string? search, CancellationToken ct = default)
    {
        var source = await LoadSourceAsync(id, importId, ct);
        if (source is null) return NotFound();
        var selectedId = source.SelectedImport.Id;
        var accounts = await dbContext.SaftAccounts.AsNoTracking().Where(x => x.SaftImportId == selectedId).Select(x => new { x.AccountId, x.Description }).ToListAsync(ct);
        var lines = await dbContext.SaftTransactionLines.AsNoTracking().Where(x => x.SaftTransaction.SaftImportId == selectedId).Select(x => new { x.AccountId, x.Side, x.Amount }).ToListAsync(ct);
        var names = accounts.ToDictionary(x => x.AccountId, x => x.Description, StringComparer.OrdinalIgnoreCase);
        var rows = lines.GroupBy(x => x.AccountId).Select(g => new AccountAnalysisRow(g.Key, names.GetValueOrDefault(g.Key) ?? "Conta não identificada no plano", g.Where(x => x.Side == "D").Sum(x => x.Amount), g.Where(x => x.Side == "C").Sum(x => x.Amount), g.Where(x => x.Side == "D").Sum(x => x.Amount) - g.Where(x => x.Side == "C").Sum(x => x.Amount), g.Count())).OrderBy(x => x.AccountId).ToList();
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim(); rows = rows.Where(x => x.AccountId.Contains(term, StringComparison.OrdinalIgnoreCase) || x.Description.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList(); }
        return View(new AccountAnalysisViewModel { Source = source, Search = search?.Trim(), Rows = rows });
    }

    private static int CountUnbalancedTransactions(IEnumerable<dynamic> lines) => lines.GroupBy(x => (int)x.SaftTransactionId).Count(g => g.Where(x => (string)x.Side == "D").Sum(x => (decimal)x.Amount) != g.Where(x => (string)x.Side == "C").Sum(x => (decimal)x.Amount));
}
