using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Services;

public interface IHomeDashboardService
{
    Task<HomeDashboardViewModel> GetAsync(CancellationToken cancellationToken);
}

public sealed class HomeDashboardService(TabulariusDbContext db) : IHomeDashboardService
{
    public async Task<HomeDashboardViewModel> GetAsync(CancellationToken ct)
    {
        var ai=await db.AiSettings.AsNoTracking().FirstOrDefaultAsync(ct);
        var dossiers=await db.AnalysisDossiers.AsNoTracking().Include(x=>x.AccountingEntity).OrderByDescending(x=>x.FiscalYear).ThenBy(x=>x.AccountingEntity.Name).ToListAsync(ct);
        var summaries=new List<HomeDossierSummary>();
        foreach(var dossier in dossiers)
        {
            var importIds=db.SaftImports.Where(x=>x.DossierId==dossier.Id).Select(x=>x.Id);
            summaries.Add(new HomeDossierSummary(dossier.Id,dossier.AccountingEntity.Name,dossier.Name,dossier.FiscalYear,
                await db.SaftImports.CountAsync(x=>x.DossierId==dossier.Id,ct),
                await db.SaftTransactions.CountAsync(x=>importIds.Contains(x.SaftImportId),ct),
                await db.SaftSalesInvoices.CountAsync(x=>importIds.Contains(x.SaftImportId),ct),
                await db.SaftSalesInvoices.Where(x=>importIds.Contains(x.SaftImportId)).SumAsync(x=>(decimal?)x.GrossTotal,ct)??0m));
        }
        var evolution=summaries.GroupBy(x=>x.FiscalYear).OrderBy(x=>x.Key).Select(group=>new HomeEvolutionPoint(group.Key,group.Sum(x=>x.Transactions),group.Sum(x=>x.GrossSales))).ToArray();
        return new HomeDashboardViewModel
        {
            Entities=await db.AccountingEntities.CountAsync(ct),Dossiers=dossiers.Count,Imports=await db.SaftImports.CountAsync(ct),Accounts=await db.SaftAccounts.CountAsync(ct),Transactions=await db.SaftTransactions.CountAsync(ct),SalesDocuments=await db.SaftSalesInvoices.CountAsync(ct),GrossSales=await db.SaftSalesInvoices.SumAsync(x=>(decimal?)x.GrossTotal,ct)??0m,
            AiEnabled=ai?.IsEnabled==true,AiProvider=ai?.Provider??string.Empty,AiModel=ai?.Model??string.Empty,AiWelcomePrompt=ai?.HomePrompt??string.Empty,DossierSummaries=summaries.Take(5).ToArray(),Evolution=evolution
        };
    }
}
