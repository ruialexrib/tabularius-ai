using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Models;
using TabulariusAI.Web.Services.AI;

namespace TabulariusAI.Web.Services;

public interface IHomeDashboardService
{
    Task<HomeDashboardViewModel> GetAsync(CancellationToken cancellationToken);
}

public sealed class HomeDashboardService(TabulariusDbContext db,IEnumerable<IAiProvider> providers,ILogger<HomeDashboardService> logger) : IHomeDashboardService
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
        var welcome=ai?.IsEnabled==true?await GenerateWelcomeAsync(ai,summaries,ct):string.Empty;
        return new HomeDashboardViewModel
        {
            Entities=await db.AccountingEntities.CountAsync(ct),Dossiers=dossiers.Count,Imports=await db.SaftImports.CountAsync(ct),Accounts=await db.SaftAccounts.CountAsync(ct),Transactions=await db.SaftTransactions.CountAsync(ct),SalesDocuments=await db.SaftSalesInvoices.CountAsync(ct),GrossSales=await db.SaftSalesInvoices.SumAsync(x=>(decimal?)x.GrossTotal,ct)??0m,
            AiEnabled=ai?.IsEnabled==true,AiProvider=ai?.Provider??string.Empty,AiModel=ai?.Model??string.Empty,AiWelcomeMessage=welcome,DossierSummaries=summaries.Take(5).ToArray(),Evolution=evolution
        };
    }

    private async Task<string> GenerateWelcomeAsync(Data.Entities.AiSettings ai,IReadOnlyList<HomeDossierSummary> summaries,CancellationToken ct)
    {
        if(summaries.Count==0)return "Ainda não existem dossiers disponíveis. Importe um ficheiro SAF-T (PT) para começar a análise.";
        var provider=providers.SingleOrDefault(x=>string.Equals(x.Name,ai.Provider,StringComparison.OrdinalIgnoreCase));
        if(provider is null)return FallbackWelcome(summaries);
        var context=JsonSerializer.Serialize(new{CurrencyCode="EUR",Dossiers=summaries.Take(10).Select(x=>new{x.Entity,x.Name,x.FiscalYear,x.Imports,x.Transactions,x.SalesDocuments,x.GrossSales})});
        var instruction=string.IsNullOrWhiteSpace(ai.HomePrompt)?"Gera uma breve mensagem de boas-vindas contextual e sugere ao utilizador uma análise útil que possa fazer de seguida.":ai.HomePrompt.Trim();
        var systemPrompt=$"És o assistente do painel inicial do Tabularius AI. Responde em português europeu. Recebes apenas um resumo determinístico dos dossiers disponíveis. Não inventes dados, não faças cálculos adicionais nem afirmes factos ausentes do contexto. Os valores monetários estão em EUR. Produz apenas uma mensagem curta, natural e profissional, com no máximo 3 frases. A última frase deve indicar claramente uma ação ou análise que o utilizador pode fazer de seguida. Instrução configurada pelo administrador: {instruction}";
        try
        {
            using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(Math.Clamp(ai.TimeoutSeconds,10,30)));
            using var linked=CancellationTokenSource.CreateLinkedTokenSource(ct,timeout.Token);
            var result=await provider.CompleteAsync(ai.Endpoint,ai.Model,ai.ApiKey,Math.Min(ai.Temperature,0.4m),systemPrompt,[new AiMessage("user",$"Resumo dos dossiers disponíveis:\n{context}")],[],linked.Token);
            return string.IsNullOrWhiteSpace(result.Content)?FallbackWelcome(summaries):result.Content.Trim();
        }
        catch(Exception exception) when(exception is HttpRequestException or InvalidOperationException or TaskCanceledException)
        {
            logger.LogWarning(exception,"Could not generate AI welcome message for the home dashboard using {Provider} and {Model}.",ai.Provider,ai.Model);
            return FallbackWelcome(summaries);
        }
    }

    private static string FallbackWelcome(IReadOnlyList<HomeDossierSummary> summaries)
    {
        var latest=summaries.OrderByDescending(x=>x.FiscalYear).First();
        return $"Tem {summaries.Count} dossier{(summaries.Count==1?"":"s")} disponível{(summaries.Count==1?"":"eis")} para análise. Pode começar pelo exercício de {latest.FiscalYear} de {latest.Entity} e explorar o balancete, as vendas ou os movimentos contabilísticos.";
    }
}
