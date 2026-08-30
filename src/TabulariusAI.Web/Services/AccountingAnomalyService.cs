using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Services;

/// <summary>Evaluates deterministic, traceable accounting controls for a single SAF-T import.</summary>
public sealed class AccountingAnomalyService(TabulariusDbContext dbContext)
{
    private const decimal BalanceTolerance = 0.01m;

    /// <summary>Evaluates all active accounting rules for the specified SAF-T import.</summary>
    public async Task<IReadOnlyList<AccountingAnomaly>> EvaluateAsync(int importId, DateOnly? startDate, DateOnly? endDate, CancellationToken ct = default)
    {
        var transactions = await dbContext.SaftTransactions.AsNoTracking()
            .Where(x => x.SaftImportId == importId)
            .Select(x => new TransactionData(x.Id, x.TransactionId, x.TransactionDate, x.Description))
            .ToListAsync(ct);
        var lines = await dbContext.SaftTransactionLines.AsNoTracking()
            .Where(x => x.SaftTransaction.SaftImportId == importId)
            .Select(x => new LineData(x.SaftTransactionId, x.RecordId, x.AccountId, x.Side, x.Amount))
            .ToListAsync(ct);
        var accountIds = (await dbContext.SaftAccounts.AsNoTracking()
            .Where(x => x.SaftImportId == importId)
            .Select(x => x.AccountId)
            .ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var findings = new List<AccountingAnomaly>();
        AddUnbalancedTransactions(findings, transactions, lines);
        AddNegativeAmounts(findings, lines);
        AddInvalidSides(findings, lines);
        AddDuplicateTransactionIds(findings, transactions);
        AddOutOfPeriodDates(findings, transactions, startDate, endDate);
        AddUnknownAccounts(findings, lines, accountIds);
        return findings.OrderBy(x => SeverityRank(x.Severity)).ThenBy(x => x.RuleId).ThenBy(x => x.Reference).ToList();
    }

    private static void AddUnbalancedTransactions(List<AccountingAnomaly> findings, IReadOnlyList<TransactionData> transactions, IReadOnlyList<LineData> lines)
    {
        foreach (var transaction in transactions)
        {
            var txLines = lines.Where(x => x.TransactionLocalId == transaction.Id);
            var debit = txLines.Where(x => x.Side == "D").Sum(x => x.Amount);
            var credit = txLines.Where(x => x.Side == "C").Sum(x => x.Amount);
            var difference = debit - credit;
            if (Math.Abs(difference) >= BalanceTolerance)
                findings.Add(new("ACC-001", "Alta", "Lançamento desequilibrado", transaction.TransactionId, $"Débitos e créditos do lançamento não coincidem. {transaction.Description}", difference, transaction.Id));
        }
    }

    private static void AddNegativeAmounts(List<AccountingAnomaly> findings, IEnumerable<LineData> lines)
    {
        findings.AddRange(lines.Where(x => x.Amount < 0).Select(x => new AccountingAnomaly("ACC-002", "Média", "Montante negativo", x.RecordId, $"A linha da conta {x.AccountId} apresenta um montante negativo.", x.Amount, x.TransactionLocalId)));
    }

    private static void AddInvalidSides(List<AccountingAnomaly> findings, IEnumerable<LineData> lines)
    {
        findings.AddRange(lines.Where(x => x.Side is not ("D" or "C")).Select(x => new AccountingAnomaly("ACC-003", "Alta", "Natureza inválida", x.RecordId, $"A linha da conta {x.AccountId} não está identificada como débito ou crédito.", null, x.TransactionLocalId)));
    }

    private static void AddDuplicateTransactionIds(List<AccountingAnomaly> findings, IEnumerable<TransactionData> transactions)
    {
        findings.AddRange(transactions.GroupBy(x => x.TransactionId, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => new AccountingAnomaly("ACC-004", "Média", "Identificador duplicado", g.Key, $"O identificador de lançamento ocorre {g.Count()} vezes nesta fonte SAF-T (PT).", null, g.First().Id)));
    }

    private static void AddOutOfPeriodDates(List<AccountingAnomaly> findings, IEnumerable<TransactionData> transactions, DateOnly? startDate, DateOnly? endDate)
    {
        if (!startDate.HasValue || !endDate.HasValue) return;
        findings.AddRange(transactions.Where(x => x.Date < startDate.Value || x.Date > endDate.Value).Select(x => new AccountingAnomaly("ACC-005", "Alta", "Data fora do período", x.TransactionId, $"O lançamento tem data {x.Date:dd/MM/yyyy}, fora do período SAF-T (PT) {startDate:dd/MM/yyyy} — {endDate:dd/MM/yyyy}.", null, x.Id)));
    }

    private static void AddUnknownAccounts(List<AccountingAnomaly> findings, IEnumerable<LineData> lines, IReadOnlySet<string> accountIds)
    {
        findings.AddRange(lines
            .Where(x => !string.IsNullOrWhiteSpace(x.AccountId) && !accountIds.Contains(x.AccountId))
            .Select(x => new AccountingAnomaly("ACC-006", "Alta", "Conta inexistente no plano", x.RecordId, $"A linha referencia a conta {x.AccountId}, que não existe no plano de contas desta fonte SAF-T (PT).", null, x.TransactionLocalId)));
    }

    private static int SeverityRank(string severity) => severity == "Alta" ? 0 : severity == "Média" ? 1 : 2;
    private sealed record TransactionData(int Id, string TransactionId, DateOnly Date, string Description);
    private sealed record LineData(int TransactionLocalId, string RecordId, string AccountId, string Side, decimal Amount);
}
