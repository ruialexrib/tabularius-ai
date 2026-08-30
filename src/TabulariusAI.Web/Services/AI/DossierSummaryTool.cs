using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TabulariusAI.Web.Data;

namespace TabulariusAI.Web.Services.AI;

/// <summary>Returns deterministic persisted counts and totals for the selected dossier.</summary>
public sealed class DossierSummaryTool(TabulariusDbContext dbContext) : IAiTool
{
    public AiToolDefinition Definition { get; } = new("get_dossier_summary", "Obtém um resumo determinístico dos dados SAF-T persistidos no dossier selecionado.", new { type="object", properties=new Dictionary<string,object>(), required=Array.Empty<string>(), additionalProperties=false });

    public async Task<object> ExecuteAsync(int dossierId, JsonElement arguments, CancellationToken cancellationToken)
    {
        var dossier = await dbContext.AnalysisDossiers.AsNoTracking().Include(item=>item.AccountingEntity).SingleAsync(item=>item.Id==dossierId,cancellationToken);
        var importIds = dbContext.SaftImports.Where(item=>item.DossierId==dossierId).Select(item=>item.Id);
        return new
        {
            dossier.Id,
            dossier.Name,
            dossier.FiscalYear,
            Entity=dossier.AccountingEntity.Name,
            Accounts=await dbContext.SaftAccounts.CountAsync(item=>importIds.Contains(item.SaftImportId),cancellationToken),
            Customers=await dbContext.SaftCustomers.CountAsync(item=>importIds.Contains(item.SaftImportId),cancellationToken),
            Suppliers=await dbContext.SaftSuppliers.CountAsync(item=>importIds.Contains(item.SaftImportId),cancellationToken),
            Products=await dbContext.SaftProducts.CountAsync(item=>importIds.Contains(item.SaftImportId),cancellationToken),
            Transactions=await dbContext.SaftTransactions.CountAsync(item=>importIds.Contains(item.SaftImportId),cancellationToken),
            SalesInvoices=await dbContext.SaftSalesInvoices.CountAsync(item=>importIds.Contains(item.SaftImportId),cancellationToken),
            GrossSales=await dbContext.SaftSalesInvoices.Where(item=>importIds.Contains(item.SaftImportId)).SumAsync(item=>(decimal?)item.GrossTotal,cancellationToken) ?? 0m,
            TaxPayable=await dbContext.SaftSalesInvoices.Where(item=>importIds.Contains(item.SaftImportId)).SumAsync(item=>(decimal?)item.TaxPayable,cancellationToken) ?? 0m,
            StockMovements=await dbContext.SaftStockMovements.CountAsync(item=>importIds.Contains(item.SaftImportId),cancellationToken),
            TaxEntries=await dbContext.SaftTaxEntries.CountAsync(item=>importIds.Contains(item.SaftImportId),cancellationToken)
        };
    }
}
