using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TabulariusAI.Web.Data;

namespace TabulariusAI.Web.Services.AI;

internal static class AiToolArguments
{
    public static string? String(JsonElement args,string name)=>args.ValueKind==JsonValueKind.Object&&args.TryGetProperty(name,out var value)&&value.ValueKind==JsonValueKind.String?value.GetString()?.Trim():null;
    public static int Limit(JsonElement args,int fallback=20,int max=100)=>args.ValueKind==JsonValueKind.Object&&args.TryGetProperty("limit",out var value)&&value.TryGetInt32(out var limit)?Math.Clamp(limit,1,max):fallback;
    public static DateOnly? Date(JsonElement args,string name)=>DateOnly.TryParse(String(args,name),out var date)?date:null;
    public static object EmptySchema()=>new { type="object",properties=new Dictionary<string,object>(),required=Array.Empty<string>(),additionalProperties=false };
    public static object SearchSchema(string property,string description)=>new { type="object",properties=new Dictionary<string,object>{{property,new { type="string",description }},{"limit",new { type="integer",minimum=1,maximum=100 }}},required=new[]{property},additionalProperties=false };
    public static object Currency()=>new { CurrencyCode="EUR",CurrencySymbol="€" };
}

public sealed class TrialBalanceTool(TabulariusDbContext db) : IAiTool
{
    public AiToolDefinition Definition { get; }=new("get_trial_balance","Obtém o balancete determinístico do dossier: saldos de abertura e fecho por conta, em EUR.",new { type="object",properties=new Dictionary<string,object>{{"limit",new { type="integer",minimum=1,maximum=100 }}},required=Array.Empty<string>(),additionalProperties=false });
    public async Task<object> ExecuteAsync(int dossierId,JsonElement args,CancellationToken ct){var ids=db.SaftImports.Where(x=>x.DossierId==dossierId).Select(x=>x.Id);var limit=AiToolArguments.Limit(args,100);var rows=await db.SaftAccounts.AsNoTracking().Where(x=>ids.Contains(x.SaftImportId)).OrderBy(x=>x.AccountId).Take(limit).Select(x=>new{x.AccountId,x.Description,x.OpeningDebitBalance,x.OpeningCreditBalance,x.ClosingDebitBalance,x.ClosingCreditBalance,ClosingBalance=x.ClosingDebitBalance-x.ClosingCreditBalance}).ToListAsync(ct);return new{CurrencyCode="EUR",CurrencySymbol="€",Count=rows.Count,Accounts=rows};}
}

public sealed class AccountBalanceTool(TabulariusDbContext db) : IAiTool
{
    public AiToolDefinition Definition { get; }=new("get_account_balance","Obtém saldos e movimentos agregados de uma conta específica do dossier.",AiToolArguments.SearchSchema("accountId","Código exato da conta contabilística."));
    public async Task<object> ExecuteAsync(int dossierId,JsonElement args,CancellationToken ct){var accountId=AiToolArguments.String(args,"accountId")??throw new InvalidOperationException("accountId é obrigatório.");var ids=db.SaftImports.Where(x=>x.DossierId==dossierId).Select(x=>x.Id);var accounts=await db.SaftAccounts.AsNoTracking().Where(x=>ids.Contains(x.SaftImportId)&&x.AccountId==accountId).Select(x=>new{x.AccountId,x.Description,x.OpeningDebitBalance,x.OpeningCreditBalance,x.ClosingDebitBalance,x.ClosingCreditBalance}).ToListAsync(ct);var lines=db.SaftTransactionLines.AsNoTracking().Where(x=>x.AccountId==accountId&&ids.Contains(x.SaftTransaction.SaftImportId));var debit=await lines.Where(x=>x.Side=="D").SumAsync(x=>(decimal?)x.Amount,ct)??0m;var credit=await lines.Where(x=>x.Side=="C").SumAsync(x=>(decimal?)x.Amount,ct)??0m;return new{CurrencyCode="EUR",CurrencySymbol="€",AccountId=accountId,Definitions=accounts,DebitMovements=debit,CreditMovements=credit,MovementBalance=debit-credit};}
}

public sealed class SearchAccountsTool(TabulariusDbContext db) : IAiTool
{
    public AiToolDefinition Definition { get; }=new("search_accounts","Procura contas do dossier por código ou descrição.",AiToolArguments.SearchSchema("query","Código, prefixo ou texto da descrição da conta."));
    public async Task<object> ExecuteAsync(int dossierId,JsonElement args,CancellationToken ct){var q=AiToolArguments.String(args,"query")??throw new InvalidOperationException("query é obrigatório.");var ids=db.SaftImports.Where(x=>x.DossierId==dossierId).Select(x=>x.Id);var limit=AiToolArguments.Limit(args);var rows=await db.SaftAccounts.AsNoTracking().Where(x=>ids.Contains(x.SaftImportId)&&(x.AccountId.Contains(q)||x.Description.Contains(q))).OrderBy(x=>x.AccountId).Take(limit).Select(x=>new{x.AccountId,x.Description,x.TaxonomyReference,x.ClosingDebitBalance,x.ClosingCreditBalance}).ToListAsync(ct);return new{CurrencyCode="EUR",CurrencySymbol="€",Results=rows};}
}

public sealed class CustomersTool(TabulariusDbContext db) : IAiTool
{
    public AiToolDefinition Definition { get; }=new("get_customers","Consulta clientes persistidos no dossier. Pode filtrar por nome, NIF ou identificador.",new { type="object",properties=new Dictionary<string,object>{{"query",new{type="string"}},{"limit",new{type="integer",minimum=1,maximum=100}}},required=Array.Empty<string>(),additionalProperties=false });
    public async Task<object> ExecuteAsync(int dossierId,JsonElement args,CancellationToken ct){var q=AiToolArguments.String(args,"query");var ids=db.SaftImports.Where(x=>x.DossierId==dossierId).Select(x=>x.Id);var query=db.SaftCustomers.AsNoTracking().Where(x=>ids.Contains(x.SaftImportId));if(!string.IsNullOrWhiteSpace(q))query=query.Where(x=>x.CustomerId.Contains(q)||x.CompanyName.Contains(q)||x.TaxId.Contains(q));var rows=await query.OrderBy(x=>x.CompanyName).Take(AiToolArguments.Limit(args)).Select(x=>new{x.CustomerId,x.CompanyName,x.TaxId,x.AccountId}).ToListAsync(ct);return new{Count=rows.Count,Customers=rows};}
}

public sealed class SuppliersTool(TabulariusDbContext db) : IAiTool
{
    public AiToolDefinition Definition { get; }=new("get_suppliers","Consulta fornecedores persistidos no dossier. Pode filtrar por nome, NIF ou identificador.",new { type="object",properties=new Dictionary<string,object>{{"query",new{type="string"}},{"limit",new{type="integer",minimum=1,maximum=100}}},required=Array.Empty<string>(),additionalProperties=false });
    public async Task<object> ExecuteAsync(int dossierId,JsonElement args,CancellationToken ct){var q=AiToolArguments.String(args,"query");var ids=db.SaftImports.Where(x=>x.DossierId==dossierId).Select(x=>x.Id);var query=db.SaftSuppliers.AsNoTracking().Where(x=>ids.Contains(x.SaftImportId));if(!string.IsNullOrWhiteSpace(q))query=query.Where(x=>x.SupplierId.Contains(q)||x.CompanyName.Contains(q)||x.TaxId.Contains(q));var rows=await query.OrderBy(x=>x.CompanyName).Take(AiToolArguments.Limit(args)).Select(x=>new{x.SupplierId,x.CompanyName,x.TaxId,x.AccountId}).ToListAsync(ct);return new{Count=rows.Count,Suppliers=rows};}
}

public sealed class ProductsTool(TabulariusDbContext db) : IAiTool
{
    public AiToolDefinition Definition { get; }=new("get_products","Consulta produtos/serviços persistidos no dossier por código ou descrição.",new { type="object",properties=new Dictionary<string,object>{{"query",new{type="string"}},{"limit",new{type="integer",minimum=1,maximum=100}}},required=Array.Empty<string>(),additionalProperties=false });
    public async Task<object> ExecuteAsync(int dossierId,JsonElement args,CancellationToken ct){var q=AiToolArguments.String(args,"query");var ids=db.SaftImports.Where(x=>x.DossierId==dossierId).Select(x=>x.Id);var query=db.SaftProducts.AsNoTracking().Where(x=>ids.Contains(x.SaftImportId));if(!string.IsNullOrWhiteSpace(q))query=query.Where(x=>x.ProductCode.Contains(q)||x.ProductDescription.Contains(q));var rows=await query.OrderBy(x=>x.ProductCode).Take(AiToolArguments.Limit(args)).Select(x=>new{x.ProductCode,x.ProductDescription,x.ProductType,x.ProductGroup,x.ProductNumberCode}).ToListAsync(ct);return new{Count=rows.Count,Products=rows};}
}

public sealed class SalesSummaryTool(TabulariusDbContext db) : IAiTool
{
    public AiToolDefinition Definition { get; }=new("get_sales_summary","Obtém indicadores determinísticos de faturação/vendas do dossier, em EUR.",AiToolArguments.EmptySchema());
    public async Task<object> ExecuteAsync(int dossierId,JsonElement args,CancellationToken ct){var ids=db.SaftImports.Where(x=>x.DossierId==dossierId).Select(x=>x.Id);var q=db.SaftSalesInvoices.AsNoTracking().Where(x=>ids.Contains(x.SaftImportId));return new{CurrencyCode="EUR",CurrencySymbol="€",DocumentLabel="Faturas/documentos de venda",Documents=await q.CountAsync(ct),NetTotal=await q.SumAsync(x=>(decimal?)x.NetTotal,ct)??0m,TaxPayable=await q.SumAsync(x=>(decimal?)x.TaxPayable,ct)??0m,GrossTotal=await q.SumAsync(x=>(decimal?)x.GrossTotal,ct)??0m,FirstDate=await q.MinAsync(x=>(DateOnly?)x.InvoiceDate,ct),LastDate=await q.MaxAsync(x=>(DateOnly?)x.InvoiceDate,ct)};}
}

public sealed class SalesInvoicesTool(TabulariusDbContext db) : IAiTool
{
    public AiToolDefinition Definition { get; }=new("get_sales_invoices","Consulta faturas/documentos de venda. Permite filtrar por cliente e intervalo de datas.",new { type="object",properties=new Dictionary<string,object>{{"customerId",new{type="string"}},{"fromDate",new{type="string",description="Data inicial YYYY-MM-DD"}},{"toDate",new{type="string",description="Data final YYYY-MM-DD"}},{"limit",new{type="integer",minimum=1,maximum=100}}},required=Array.Empty<string>(),additionalProperties=false });
    public async Task<object> ExecuteAsync(int dossierId,JsonElement args,CancellationToken ct){var ids=db.SaftImports.Where(x=>x.DossierId==dossierId).Select(x=>x.Id);var q=db.SaftSalesInvoices.AsNoTracking().Where(x=>ids.Contains(x.SaftImportId));var customer=AiToolArguments.String(args,"customerId");var from=AiToolArguments.Date(args,"fromDate");var to=AiToolArguments.Date(args,"toDate");if(!string.IsNullOrWhiteSpace(customer))q=q.Where(x=>x.CustomerId==customer);if(from.HasValue)q=q.Where(x=>x.InvoiceDate>=from.Value);if(to.HasValue)q=q.Where(x=>x.InvoiceDate<=to.Value);var rows=await q.OrderByDescending(x=>x.InvoiceDate).ThenBy(x=>x.InvoiceNo).Take(AiToolArguments.Limit(args)).Select(x=>new{x.InvoiceNo,x.InvoiceDate,x.InvoiceType,x.InvoiceStatus,x.CustomerId,x.NetTotal,x.TaxPayable,x.GrossTotal}).ToListAsync(ct);return new{CurrencyCode="EUR",CurrencySymbol="€",DocumentLabel="Faturas/documentos de venda",Count=rows.Count,Documents=rows};}
}

public sealed class StockMovementsTool(TabulariusDbContext db) : IAiTool
{
    public AiToolDefinition Definition { get; }=new("get_stock_movements","Consulta documentos de movimentação de mercadorias do dossier.",new { type="object",properties=new Dictionary<string,object>{{"limit",new{type="integer",minimum=1,maximum=100}}},required=Array.Empty<string>(),additionalProperties=false });
    public async Task<object> ExecuteAsync(int dossierId,JsonElement args,CancellationToken ct){var ids=db.SaftImports.Where(x=>x.DossierId==dossierId).Select(x=>x.Id);var rows=await db.SaftStockMovements.AsNoTracking().Where(x=>ids.Contains(x.SaftImportId)).OrderByDescending(x=>x.MovementDate).Take(AiToolArguments.Limit(args)).Select(x=>new{x.DocumentNumber,x.MovementDate,x.MovementType,x.DocumentStatus,x.CustomerId,x.SupplierId}).ToListAsync(ct);return new{Count=rows.Count,Movements=rows};}
}

public sealed class TaxTableTool(TabulariusDbContext db) : IAiTool
{
    public AiToolDefinition Definition { get; }=new("get_tax_table","Consulta a tabela de impostos SAF-T persistida no dossier.",new { type="object",properties=new Dictionary<string,object>{{"taxType",new{type="string"}},{"limit",new{type="integer",minimum=1,maximum=100}}},required=Array.Empty<string>(),additionalProperties=false });
    public async Task<object> ExecuteAsync(int dossierId,JsonElement args,CancellationToken ct){var ids=db.SaftImports.Where(x=>x.DossierId==dossierId).Select(x=>x.Id);var q=db.SaftTaxEntries.AsNoTracking().Where(x=>ids.Contains(x.SaftImportId));var type=AiToolArguments.String(args,"taxType");if(!string.IsNullOrWhiteSpace(type))q=q.Where(x=>x.TaxType==type);var rows=await q.OrderBy(x=>x.TaxType).ThenBy(x=>x.TaxCode).Take(AiToolArguments.Limit(args)).Select(x=>new{x.TaxType,x.TaxCountryRegion,x.TaxCode,x.Description,x.TaxPercentage,x.TaxAmount}).ToListAsync(ct);return new{CurrencyCode="EUR",CurrencySymbol="€",Count=rows.Count,Taxes=rows};}
}
