using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Services;

/// <summary>Evaluates deterministic, traceable accounting controls for a single SAF-T import.</summary>
public sealed partial class AccountingAnomalyService(TabulariusDbContext dbContext)
{
    private const decimal BalanceTolerance = 0.01m;
    private const int UnusualAmountMinimumPopulation = 5;
    private const decimal UnusualAmountIqrMultiplier = 3m;

    public async Task<IReadOnlyList<AccountingAnomaly>> EvaluateAsync(int importId, DateOnly? startDate, DateOnly? endDate, CancellationToken ct = default)
    {
        var transactions = await dbContext.SaftTransactions.AsNoTracking().Where(x => x.SaftImportId == importId).Select(x => new TransactionData(x.Id, x.TransactionId, x.TransactionDate, x.Description)).ToListAsync(ct);
        var lines = await dbContext.SaftTransactionLines.AsNoTracking().Where(x => x.SaftTransaction.SaftImportId == importId).Select(x => new LineData(x.SaftTransactionId, x.RecordId, x.AccountId, x.Side, x.Amount, x.SourceDocumentId)).ToListAsync(ct);
        var accountIds = (await dbContext.SaftAccounts.AsNoTracking().Where(x => x.SaftImportId == importId).Select(x => x.AccountId).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var taxEntries = await dbContext.SaftTaxEntries.AsNoTracking().Where(x => x.SaftImportId == importId).Select(x => new TaxEntryData(x.TaxType, x.TaxCode, x.TaxPercentage)).ToListAsync(ct);
        var invoiceTaxLines = await dbContext.SaftSalesInvoiceLines.AsNoTracking().Where(x => x.SaftSalesInvoice.SaftImportId == importId && x.TaxType != null && x.TaxCode != null).Select(x => new InvoiceTaxLineData(x.SaftSalesInvoice.InvoiceNo, x.LineNumber, x.TaxType!, x.TaxCode!, x.TaxPercentage)).ToListAsync(ct);
        var invoices = await dbContext.SaftSalesInvoices.AsNoTracking().Where(x => x.SaftImportId == importId).Select(x => new DocumentData(x.InvoiceNo, x.InvoiceType, x.InvoiceDate, x.GrossTotal)).ToListAsync(ct);
        var payments = await dbContext.SaftPayments.AsNoTracking().Where(x => x.SaftImportId == importId).Select(x => new DocumentData(x.PaymentRefNo, x.PaymentType, x.TransactionDate, x.GrossTotal)).ToListAsync(ct);

        var findings = new List<AccountingAnomaly>();
        AddUnbalancedTransactions(findings, transactions, lines);
        AddNegativeAmounts(findings, lines);
        AddInvalidSides(findings, lines);
        AddDuplicateTransactionIds(findings, transactions);
        AddOutOfPeriodDates(findings, transactions, startDate, endDate);
        AddUnknownAccounts(findings, lines, accountIds);
        AddUnusualAmounts(findings, lines, accountIds);
        AddTaxInconsistencies(findings, invoiceTaxLines, taxEntries);
        AddDocumentReconciliation(findings, "ACC-009", "Fatura não reconciliada", "Fatura com diferença contabilística", invoices, lines);
        AddDocumentReconciliation(findings, "ACC-010", "Pagamento não reconciliado", "Pagamento com diferença contabilística", payments, lines);
        AddSequenceGaps(findings, invoices);
        return findings.OrderBy(x => SeverityRank(x.Severity)).ThenBy(x => x.RuleId).ThenBy(x => x.Reference).ToList();
    }

    private static void AddUnbalancedTransactions(List<AccountingAnomaly> findings, IReadOnlyList<TransactionData> transactions, IReadOnlyList<LineData> lines)
    {
        foreach (var transaction in transactions)
        {
            var txLines = lines.Where(x => x.TransactionLocalId == transaction.Id);
            var debit = txLines.Where(x => x.Side == "D").Sum(x => x.Amount); var credit = txLines.Where(x => x.Side == "C").Sum(x => x.Amount); var difference = debit - credit;
            if (Math.Abs(difference) >= BalanceTolerance) findings.Add(new("ACC-001", "Alta", "Lançamento desequilibrado", transaction.TransactionId, $"Débitos e créditos do lançamento não coincidem. {transaction.Description}", difference, transaction.Id));
        }
    }
    private static void AddNegativeAmounts(List<AccountingAnomaly> findings, IEnumerable<LineData> lines) => findings.AddRange(lines.Where(x => x.Amount < 0).Select(x => new AccountingAnomaly("ACC-002", "Média", "Montante negativo", x.RecordId, $"A linha da conta {x.AccountId} apresenta um montante negativo.", x.Amount, x.TransactionLocalId)));
    private static void AddInvalidSides(List<AccountingAnomaly> findings, IEnumerable<LineData> lines) => findings.AddRange(lines.Where(x => x.Side is not ("D" or "C")).Select(x => new AccountingAnomaly("ACC-003", "Alta", "Natureza inválida", x.RecordId, $"A linha da conta {x.AccountId} não está identificada como débito ou crédito.", null, x.TransactionLocalId)));
    private static void AddDuplicateTransactionIds(List<AccountingAnomaly> findings, IEnumerable<TransactionData> transactions) => findings.AddRange(transactions.GroupBy(x => x.TransactionId, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => new AccountingAnomaly("ACC-004", "Média", "Identificador duplicado", g.Key, $"O identificador de lançamento ocorre {g.Count()} vezes nesta fonte SAF-T (PT).", null, g.First().Id)));
    private static void AddOutOfPeriodDates(List<AccountingAnomaly> findings, IEnumerable<TransactionData> transactions, DateOnly? startDate, DateOnly? endDate){if(!startDate.HasValue||!endDate.HasValue)return;findings.AddRange(transactions.Where(x=>x.Date<startDate.Value||x.Date>endDate.Value).Select(x=>new AccountingAnomaly("ACC-005","Alta","Data fora do período",x.TransactionId,$"O lançamento tem data {x.Date:dd/MM/yyyy}, fora do período SAF-T (PT) {startDate:dd/MM/yyyy} — {endDate:dd/MM/yyyy}.",null,x.Id)));}
    private static void AddUnknownAccounts(List<AccountingAnomaly> findings,IEnumerable<LineData> lines,IReadOnlySet<string> accountIds)=>findings.AddRange(lines.Where(x=>!string.IsNullOrWhiteSpace(x.AccountId)&&!accountIds.Contains(x.AccountId)).Select(x=>new AccountingAnomaly("ACC-006","Alta","Conta inexistente no plano",x.RecordId,$"A linha referencia a conta {x.AccountId}, que não existe no plano de contas desta fonte SAF-T (PT).",null,x.TransactionLocalId)));
    private static void AddUnusualAmounts(List<AccountingAnomaly> findings,IEnumerable<LineData> lines,IReadOnlySet<string> accountIds){foreach(var group in lines.Where(x=>accountIds.Contains(x.AccountId)&&x.Amount>=0&&x.Side is "D" or "C").GroupBy(x=>x.AccountId,StringComparer.OrdinalIgnoreCase)){var ordered=group.Select(x=>x.Amount).OrderBy(x=>x).ToArray();if(ordered.Length<UnusualAmountMinimumPopulation)continue;var q1=Percentile(ordered,.25m);var q3=Percentile(ordered,.75m);var iqr=q3-q1;if(iqr<=0)continue;var upperFence=q3+UnusualAmountIqrMultiplier*iqr;findings.AddRange(group.Where(x=>x.Amount>upperFence).Select(x=>new AccountingAnomaly("ACC-007","Média","Montante invulgar na conta",x.RecordId,$"O montante {x.Amount:N2} € na conta {x.AccountId} excede o limite estatístico {upperFence:N2} € (Q3 + 3×IQR), calculado sobre {ordered.Length} movimentos da própria conta.",x.Amount-upperFence,x.TransactionLocalId)));}}
    private static void AddTaxInconsistencies(List<AccountingAnomaly> findings,IEnumerable<InvoiceTaxLineData> lines,IReadOnlyList<TaxEntryData> taxEntries){foreach(var line in lines.Where(x=>!string.IsNullOrWhiteSpace(x.TaxType)&&!string.IsNullOrWhiteSpace(x.TaxCode))){var definitions=taxEntries.Where(x=>string.Equals(x.TaxType,line.TaxType,StringComparison.OrdinalIgnoreCase)&&string.Equals(x.TaxCode,line.TaxCode,StringComparison.OrdinalIgnoreCase)).ToList();var reference=$"{line.InvoiceNo} · linha {line.LineNumber}";if(definitions.Count==0){findings.Add(new("ACC-008","Alta","Código fiscal não definido",reference,$"A fatura {line.InvoiceNo}, linha {line.LineNumber}, usa {line.TaxType}/{line.TaxCode}, combinação inexistente na TaxTable desta fonte SAF-T (PT).",null,null));continue;}if(line.TaxPercentage.HasValue&&definitions.Any(x=>x.TaxPercentage.HasValue)&&!definitions.Any(x=>x.TaxPercentage==line.TaxPercentage)){var expected=string.Join(" / ",definitions.Where(x=>x.TaxPercentage.HasValue).Select(x=>$"{x.TaxPercentage:0.####}%").Distinct());findings.Add(new("ACC-008","Alta","Taxa fiscal inconsistente",reference,$"A fatura {line.InvoiceNo}, linha {line.LineNumber}, declara {line.TaxPercentage:0.####}% para {line.TaxType}/{line.TaxCode}; a TaxTable define {expected}.",null,null));}}}

    private static void AddDocumentReconciliation(List<AccountingAnomaly> findings,string ruleId,string missingType,string differenceType,IEnumerable<DocumentData> documents,IReadOnlyList<LineData> lines)
    {
        foreach(var document in documents.Where(x=>!string.IsNullOrWhiteSpace(x.Number)&&x.GrossTotal!=0))
        {
            var matched=lines.Where(x=>string.Equals(x.SourceDocumentId,document.Number,StringComparison.OrdinalIgnoreCase)).ToList();
            if(matched.Count==0){findings.Add(new(ruleId,"Média",missingType,document.Number,$"O documento {document.Number} ({document.Date:dd/MM/yyyy}) não tem linhas contabilísticas com SourceDocumentID correspondente.",null,null));continue;}
            var debit=matched.Where(x=>x.Side=="D").Sum(x=>x.Amount);var credit=matched.Where(x=>x.Side=="C").Sum(x=>x.Amount);var expected=Math.Abs(document.GrossTotal);
            var bestDifference=new[]{Math.Abs(debit-expected),Math.Abs(credit-expected)}.Min();
            if(bestDifference>=BalanceTolerance)findings.Add(new(ruleId,"Alta",differenceType,document.Number,$"O documento {document.Number} tem total bruto {expected:N2} €, mas os movimentos contabilísticos associados totalizam {debit:N2} € a débito e {credit:N2} € a crédito.",bestDifference,matched.First().TransactionLocalId));
        }
    }

    private static void AddSequenceGaps(List<AccountingAnomaly> findings,IEnumerable<DocumentData> invoices)
    {
        var parsed=invoices.Select(x=>(Document:x,Match:TrailingNumber().Match(x.Number))).Where(x=>x.Match.Success).Select(x=>new SequenceData(x.Document,x.Match.Groups[1].Value,int.Parse(x.Match.Groups[2].Value))).ToList();
        foreach(var group in parsed.GroupBy(x=>new{x.Document.Type,x.Prefix}))
        {
            var numbers=group.Select(x=>x.Number).Distinct().OrderBy(x=>x).ToArray();if(numbers.Length<2)continue;
            var set=numbers.ToHashSet();
            for(var number=numbers[0]+1;number<numbers[^1];number++)if(!set.Contains(number)){var previous=group.Where(x=>x.Number<number).OrderByDescending(x=>x.Number).First();var next=group.Where(x=>x.Number>number).OrderBy(x=>x.Number).First();findings.Add(new("ACC-011","Média","Lacuna na sequência documental",$"{group.Key.Prefix}{number}",$"Foi detetada uma lacuna entre {previous.Document.Number} e {next.Document.Number} na série documental {group.Key.Prefix.Trim()} ({group.Key.Type}).",null,null));}
        }
    }

    private static decimal Percentile(IReadOnlyList<decimal> ordered,decimal percentile){if(ordered.Count==1)return ordered[0];var position=percentile*(ordered.Count-1);var lower=(int)Math.Floor(position);var upper=(int)Math.Ceiling(position);if(lower==upper)return ordered[lower];var fraction=position-lower;return ordered[lower]+(ordered[upper]-ordered[lower])*fraction;}
    private static int SeverityRank(string severity)=>severity=="Alta"?0:severity=="Média"?1:2;
    [GeneratedRegex(@"^(.*?)(\d+)$",RegexOptions.CultureInvariant)] private static partial Regex TrailingNumber();
    private sealed record TransactionData(int Id,string TransactionId,DateOnly Date,string Description);
    private sealed record LineData(int TransactionLocalId,string RecordId,string AccountId,string Side,decimal Amount,string? SourceDocumentId);
    private sealed record TaxEntryData(string TaxType,string TaxCode,decimal? TaxPercentage);
    private sealed record InvoiceTaxLineData(string InvoiceNo,string LineNumber,string TaxType,string TaxCode,decimal? TaxPercentage);
    private sealed record DocumentData(string Number,string Type,DateOnly Date,decimal GrossTotal);
    private sealed record SequenceData(DocumentData Document,string Prefix,int Number);
}
