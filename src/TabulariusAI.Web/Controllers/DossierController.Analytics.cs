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
        var accountNames = accounts.ToDictionary(x => x.AccountId, x => x.Description, StringComparer.OrdinalIgnoreCase); var transactionDates = transactions.ToDictionary(x => x.Id, x => x.TransactionDate);
        var topAccounts = lines.GroupBy(x => x.AccountId).Select(g => new AccountAnalysisRow(g.Key, accountNames.GetValueOrDefault(g.Key) ?? "Conta não identificada no plano", g.Where(x => x.Side == "D").Sum(x => x.Amount), g.Where(x => x.Side == "C").Sum(x => x.Amount), g.Where(x => x.Side == "D").Sum(x => x.Amount) - g.Where(x => x.Side == "C").Sum(x => x.Amount), g.Count())).OrderByDescending(x => x.Debit + x.Credit).Take(8).ToList();
        var monthly = lines.Where(x => transactionDates.ContainsKey(x.SaftTransactionId)).GroupBy(x => transactionDates[x.SaftTransactionId].Month).Select(g => new AnalyticsMonthlyRow(g.Key, g.Where(x => x.Side == "D").Sum(x => x.Amount), g.Where(x => x.Side == "C").Sum(x => x.Amount))).OrderBy(x => x.Month).ToList();
        var anomalyCount = (await new AccountingAnomalyService(dbContext).EvaluateAsync(selectedId, source.SelectedImport.StartDate, source.SelectedImport.EndDate, ct)).Count;
        return View(new AnalyticsOverviewViewModel { Source = source, TransactionCount = transactions.Count, TotalDebit = lines.Where(x => x.Side == "D").Sum(x => x.Amount), TotalCredit = lines.Where(x => x.Side == "C").Sum(x => x.Amount), ActiveAccountCount = lines.Select(x => x.AccountId).Distinct(StringComparer.OrdinalIgnoreCase).Count(), AnomalyCount = anomalyCount, Monthly = monthly, TopAccounts = topAccounts });
    }

    public async Task<IActionResult> Anomalies(int id, int? importId, CancellationToken ct = default)
    {
        var source = await LoadSourceAsync(id, importId, ct); if (source is null) return NotFound();
        var findings = await new AccountingAnomalyService(dbContext).EvaluateAsync(source.SelectedImport.Id, source.SelectedImport.StartDate, source.SelectedImport.EndDate, ct);
        var checks = new[]
        {
            new AccountingAnomalyRuleCheck("ACC-001", "Equilíbrio do lançamento", "Verifica se o total de débitos coincide com o total de créditos em cada lançamento, com tolerância inferior a um cêntimo.", "Alta", findings.Count(x => x.RuleId == "ACC-001")),
            new AccountingAnomalyRuleCheck("ACC-002", "Montantes negativos", "Verifica se existem linhas contabilísticas com montante negativo.", "Média", findings.Count(x => x.RuleId == "ACC-002")),
            new AccountingAnomalyRuleCheck("ACC-003", "Natureza débito/crédito", "Verifica se cada linha está identificada com uma natureza válida: débito (D) ou crédito (C).", "Alta", findings.Count(x => x.RuleId == "ACC-003")),
            new AccountingAnomalyRuleCheck("ACC-004", "Identificadores duplicados", "Verifica se o mesmo identificador de lançamento ocorre mais do que uma vez na fonte SAF-T (PT).", "Média", findings.Count(x => x.RuleId == "ACC-004")),
            new AccountingAnomalyRuleCheck("ACC-005", "Datas dentro do período", "Verifica se as datas dos lançamentos se encontram dentro do período declarado no SAF-T (PT).", "Alta", findings.Count(x => x.RuleId == "ACC-005")),
            new AccountingAnomalyRuleCheck("ACC-006", "Contas existentes no plano", "Verifica se todas as contas referenciadas nas linhas contabilísticas existem no plano de contas da mesma fonte SAF-T (PT).", "Alta", findings.Count(x => x.RuleId == "ACC-006")),
            new AccountingAnomalyRuleCheck("ACC-007", "Montantes invulgares", "Verifica, em contas com histórico suficiente, montantes acima de Q3 + 3×IQR relativamente à distribuição dos movimentos da própria conta.", "Média", findings.Count(x => x.RuleId == "ACC-007")),
            new AccountingAnomalyRuleCheck("ACC-008", "Coerência fiscal das vendas", "Verifica se os códigos e taxas de imposto das linhas das faturas existem e são coerentes com a TaxTable da mesma fonte SAF-T (PT).", "Alta", findings.Count(x => x.RuleId == "ACC-008")),
            new AccountingAnomalyRuleCheck("ACC-009", "Reconciliação de faturas", "Cruza cada fatura de venda com as linhas contabilísticas através de SourceDocumentID e compara o total bruto com os totais contabilísticos associados.", "Alta", findings.Count(x => x.RuleId == "ACC-009")),
            new AccountingAnomalyRuleCheck("ACC-010", "Reconciliação de pagamentos", "Cruza pagamentos/recebimentos com as linhas contabilísticas através de SourceDocumentID e compara o total bruto com os totais contabilísticos associados.", "Alta", findings.Count(x => x.RuleId == "ACC-010")),
            new AccountingAnomalyRuleCheck("ACC-011", "Sequência documental de vendas", "Deteta lacunas internas em séries de faturação quando o número do documento termina numa componente numérica comparável.", "Média", findings.Count(x => x.RuleId == "ACC-011"))
        };
        return View(new AnomaliesViewModel { Source = source, Findings = findings, Checks = checks });
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

    public async Task<IActionResult> AccountInvestigation(int id, string accountId, int? importId, CancellationToken ct = default)
    {
        var source = await LoadSourceAsync(id, importId, ct); if (source is null) return NotFound(); var selectedId = source.SelectedImport.Id;
        var account = await dbContext.SaftAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.SaftImportId == selectedId && x.AccountId == accountId, ct);
        var movements = await dbContext.SaftTransactionLines.AsNoTracking().Where(x => x.SaftTransaction.SaftImportId == selectedId && x.AccountId == accountId).OrderByDescending(x => x.SaftTransaction.TransactionDate).ThenByDescending(x => x.SaftTransactionId).Select(x => new AccountMovementRow(x.SaftTransactionId, x.SaftTransaction.TransactionId, x.SaftTransaction.TransactionDate, x.SaftTransaction.JournalId, x.Description, x.Side, x.Amount, x.SourceDocumentId)).ToListAsync(ct);
        if (account is null && movements.Count == 0) return NotFound(); var transactionIds = movements.Select(x => x.TransactionLocalId).Distinct().ToList();
        var counterpartLines = await dbContext.SaftTransactionLines.AsNoTracking().Where(x => transactionIds.Contains(x.SaftTransactionId) && x.AccountId != accountId).Select(x => new { x.SaftTransactionId, x.AccountId, x.Amount }).ToListAsync(ct);
        var names = await dbContext.SaftAccounts.AsNoTracking().Where(x => x.SaftImportId == selectedId).ToDictionaryAsync(x => x.AccountId, x => x.Description, ct);
        var counterparts = counterpartLines.GroupBy(x => x.AccountId).Select(g => new CounterpartAccountRow(g.Key, names.GetValueOrDefault(g.Key) ?? "Conta não identificada no plano", g.Select(x => x.SaftTransactionId).Distinct().Count(), g.Sum(x => Math.Abs(x.Amount)))).OrderByDescending(x => x.MovementVolume).Take(10).ToList();
        var monthly = movements.GroupBy(x => x.Date.Month).Select(g => new AnalyticsMonthlyRow(g.Key, g.Where(x => x.Side == "D").Sum(x => x.Amount), g.Where(x => x.Side == "C").Sum(x => x.Amount))).OrderBy(x => x.Month).ToList();
        return View(new AccountInvestigationViewModel { Source = source, AccountId = accountId, Description = account?.Description ?? "Conta não identificada no plano", OpeningDebit = account?.OpeningDebitBalance ?? 0, OpeningCredit = account?.OpeningCreditBalance ?? 0, ClosingDebit = account?.ClosingDebitBalance ?? 0, ClosingCredit = account?.ClosingCreditBalance ?? 0, Debit = movements.Where(x => x.Side == "D").Sum(x => x.Amount), Credit = movements.Where(x => x.Side == "C").Sum(x => x.Amount), Monthly = monthly, Movements = movements, Counterparts = counterparts });
    }
}
