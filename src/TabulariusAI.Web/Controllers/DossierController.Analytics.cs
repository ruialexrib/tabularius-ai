using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Models;
using TabulariusAI.Web.Services;

namespace TabulariusAI.Web.Controllers;

/// <summary>Provides deterministic analytical views for the selected accounting dossier source.</summary>
public sealed partial class DossierController
{
    public async Task<IActionResult> Analytics(int id, int? importId, CancellationToken ct = default)
    {
        var source = await LoadSourceAsync(id, importId, ct); if (source is null) return NotFound();
        var selectedId = source.SelectedImport.Id;
        var transactions = await dbContext.SaftTransactions.AsNoTracking().Where(x => x.SaftImportId == selectedId).Select(x => new { x.Id, x.TransactionDate }).ToListAsync(ct);
        var lines = await dbContext.SaftTransactionLines.AsNoTracking().Where(x => x.SaftTransaction.SaftImportId == selectedId).Select(x => new { x.SaftTransactionId, x.AccountId, x.Side, x.Amount }).ToListAsync(ct);
        var accounts = await dbContext.SaftAccounts.AsNoTracking().Where(x => x.SaftImportId == selectedId).Select(x => new { x.AccountId, x.Description }).ToListAsync(ct);
        var accountNames = accounts.ToDictionary(x => x.AccountId, x => x.Description, StringComparer.OrdinalIgnoreCase);
        var transactionDates = transactions.ToDictionary(x => x.Id, x => x.TransactionDate);
        var topAccounts = lines.GroupBy(x => x.AccountId).Select(g => new AccountAnalysisRow(g.Key, accountNames.GetValueOrDefault(g.Key) ?? "Conta não identificada no plano", g.Where(x => x.Side == "D").Sum(x => x.Amount), g.Where(x => x.Side == "C").Sum(x => x.Amount), g.Where(x => x.Side == "D").Sum(x => x.Amount) - g.Where(x => x.Side == "C").Sum(x => x.Amount), g.Count())).OrderByDescending(x => x.Debit + x.Credit).Take(8).ToList();
        var monthly = lines.Where(x => transactionDates.ContainsKey(x.SaftTransactionId)).GroupBy(x => transactionDates[x.SaftTransactionId].Month).Select(g => new AnalyticsMonthlyRow(g.Key, g.Where(x => x.Side == "D").Sum(x => x.Amount), g.Where(x => x.Side == "C").Sum(x => x.Amount))).OrderBy(x => x.Month).ToList();
        var anomalyService = new AccountingAnomalyService(dbContext);
        var anomalyCount = (await anomalyService.EvaluateAsync(selectedId, source.SelectedImport.StartDate, source.SelectedImport.EndDate, ct)).Count;
        return View(new AnalyticsOverviewViewModel { Source = source, TransactionCount = transactions.Count, TotalDebit = lines.Where(x => x.Side == "D").Sum(x => x.Amount), TotalCredit = lines.Where(x => x.Side == "C").Sum(x => x.Amount), ActiveAccountCount = lines.Select(x => x.AccountId).Distinct(StringComparer.OrdinalIgnoreCase).Count(), AnomalyCount = anomalyCount, Monthly = monthly, TopAccounts = topAccounts });
    }

    public async Task<IActionResult> Anomalies(int id, int? importId, CancellationToken ct = default)
    {
        var source = await LoadSourceAsync(id, importId, ct); if (source is null) return NotFound();
        var service = new AccountingAnomalyService(dbContext);
        var findings = await service.EvaluateAsync(source.SelectedImport.Id, source.SelectedImport.StartDate, source.SelectedImport.EndDate, ct);
        return View(new AnomaliesViewModel { Source = source, Findings = findings });
    }

    public async Task<IActionResult> AccountAnalysis(int id, int? importId, string? search, CancellationToken ct = default)
    {
        var source = await LoadSourceAsync(id, importId, ct); if (source is null) return NotFound(); var selectedId = source.SelectedImport.Id;
        var accounts = await dbContext.SaftAccounts.AsNoTracking().Where(x => x.SaftImportId == selectedId).Select(x => new { x.AccountId, x.Description }).ToListAsync(ct);
        var lines = await dbContext.SaftTransactionLines.AsNoTracking().Where(x => x.SaftTransaction.SaftImportId == selectedId).Select(x => new { x.AccountId, x.Side, x.Amount }).ToListAsync(ct);
        var names = accounts.ToDictionary(x => x.AccountId, x => x.Description, StringComparer.OrdinalIgnoreCase);
        var rows = lines.GroupBy(x => x.AccountId).Select(g => new AccountAnalysisRow(g.Key, names.GetValueOrDefault(g.Key) ?? "Conta não identificada no plano", g.Where(x => x.Side == "D").Sum(x => x.Amount), g.Where(x => x.Side == "C").Sum(x => x.Amount), g.Where(x => x.Side == "D").Sum(x => x.Amount) - g.Where(x => x.Side == "C").Sum(x => x.Amount), g.Count())).OrderBy(x => x.AccountId).ToList();
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim(); rows = rows.Where(x => x.AccountId.Contains(term, StringComparison.OrdinalIgnoreCase) || x.Description.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList(); }
        return View(new AccountAnalysisViewModel { Source = source, Search = search?.Trim(), Rows = rows });
    }

    /// <summary>Displays traceable deterministic investigation for one ledger account.</summary>
    public async Task<IActionResult> AccountInvestigation(int id, string accountId, int? importId, CancellationToken ct = default)
    {
        var source = await LoadSourceAsync(id, importId, ct); if (source is null) return NotFound(); var selectedId = source.SelectedImport.Id;
        var account = await dbContext.SaftAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.SaftImportId == selectedId && x.AccountId == accountId, ct);
        var movements = await dbContext.SaftTransactionLines.AsNoTracking().Where(x => x.SaftTransaction.SaftImportId == selectedId && x.AccountId == accountId).OrderByDescending(x => x.SaftTransaction.TransactionDate).ThenByDescending(x => x.SaftTransactionId).Select(x => new AccountMovementRow(x.SaftTransactionId, x.SaftTransaction.TransactionId, x.SaftTransaction.TransactionDate, x.SaftTransaction.JournalId, x.Description, x.Side, x.Amount, x.SourceDocumentId)).ToListAsync(ct);
        if (account is null && movements.Count == 0) return NotFound();
        var transactionIds = movements.Select(x => x.TransactionLocalId).Distinct().ToList();
        var counterpartLines = await dbContext.SaftTransactionLines.AsNoTracking().Where(x => transactionIds.Contains(x.SaftTransactionId) && x.AccountId != accountId).Select(x => new { x.SaftTransactionId, x.AccountId, x.Amount }).ToListAsync(ct);
        var names = await dbContext.SaftAccounts.AsNoTracking().Where(x => x.SaftImportId == selectedId).ToDictionaryAsync(x => x.AccountId, x => x.Description, ct);
        var counterparts = counterpartLines.GroupBy(x => x.AccountId).Select(g => new CounterpartAccountRow(g.Key, names.GetValueOrDefault(g.Key) ?? "Conta não identificada no plano", g.Select(x => x.SaftTransactionId).Distinct().Count(), g.Sum(x => Math.Abs(x.Amount)))).OrderByDescending(x => x.MovementVolume).Take(10).ToList();
        var monthly = movements.GroupBy(x => x.Date.Month).Select(g => new AnalyticsMonthlyRow(g.Key, g.Where(x => x.Side == "D").Sum(x => x.Amount), g.Where(x => x.Side == "C").Sum(x => x.Amount))).OrderBy(x => x.Month).ToList();
        return View(new AccountInvestigationViewModel { Source = source, AccountId = accountId, Description = account?.Description ?? "Conta não identificada no plano", OpeningDebit = account?.OpeningDebitBalance ?? 0, OpeningCredit = account?.OpeningCreditBalance ?? 0, ClosingDebit = account?.ClosingDebitBalance ?? 0, ClosingCredit = account?.ClosingCreditBalance ?? 0, Debit = movements.Where(x => x.Side == "D").Sum(x => x.Amount), Credit = movements.Where(x => x.Side == "C").Sum(x => x.Amount), Monthly = monthly, Movements = movements, Counterparts = counterparts });
    }
}
