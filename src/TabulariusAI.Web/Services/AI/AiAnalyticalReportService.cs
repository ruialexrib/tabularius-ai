using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Data.Entities;

namespace TabulariusAI.Web.Services.AI;

public sealed record AiAnalyticalReportRequest(string Area,string Title,object Context);
public sealed record AiAnalyticalReportResult(bool IsAvailable,string? Content,string? Provider,string? Model);
public interface IAiAnalyticalReportService{Task<AiAnalyticalReportResult> GenerateAsync(AiAnalyticalReportRequest request,CancellationToken cancellationToken=default);}

/// <summary>Generates interpretation only from deterministic analytical context supplied by the application.</summary>
public sealed class AiAnalyticalReportService(TabulariusDbContext db,IEnumerable<IAiProvider> providers,ILogger<AiAnalyticalReportService> logger):IAiAnalyticalReportService
{
    public async Task<AiAnalyticalReportResult> GenerateAsync(AiAnalyticalReportRequest request,CancellationToken ct=default)
    {
        var settings=await db.AiSettings.AsNoTracking().SingleOrDefaultAsync(ct);
        if(settings?.IsEnabled!=true)return new(false,null,null,null);
        var provider=providers.SingleOrDefault(x=>string.Equals(x.Name,settings.Provider,StringComparison.OrdinalIgnoreCase));
        if(provider is null)return new(false,null,settings.Provider,settings.Model);
        var prompt=PromptFor(settings,request.Area);
        var context=JsonSerializer.Serialize(request.Context,new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase});
        var system=$"{settings.SystemPrompt}\n\nEstás a produzir um relatório interpretativo para a área '{request.Title}'. Os dados que recebes foram calculados deterministicamente pelo Tabularius AI. Não recalcules totais, não alteres valores, não inventes factos e não uses conhecimento externo para preencher lacunas. Distingue factos observados de recomendações de revisão. Responde em português europeu, em Markdown simples, com um título curto e 2 a 4 secções concisas. Prompt específico configurado pelo administrador: {prompt}";
        try{using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(Math.Clamp(settings.TimeoutSeconds,10,120)));using var linked=CancellationTokenSource.CreateLinkedTokenSource(ct,timeout.Token);var response=await provider.CompleteAsync(settings.Endpoint,settings.Model,settings.ApiKey,settings.Temperature,system,[new AiMessage("user",$"Produz o relatório com base exclusivamente neste contexto determinístico em JSON:\n{context}")],[],linked.Token);return new(true,string.IsNullOrWhiteSpace(response.Content)?"Não foi possível gerar conteúdo interpretativo para estes dados.":response.Content.Trim(),settings.Provider,settings.Model);}
        catch(Exception ex) when(ex is HttpRequestException or InvalidOperationException or TaskCanceledException){logger.LogWarning(ex,"Could not generate analytical AI report {Area} using {Provider}/{Model}.",request.Area,settings.Provider,settings.Model);return new(true,"O relatório de AI está temporariamente indisponível. Os indicadores determinísticos desta página permanecem válidos.",settings.Provider,settings.Model);}
    }
    private static string PromptFor(AiSettings s,string area)=>area switch{"analytics-overview"=>s.AnalyticsOverviewPrompt,"anomalies"=>s.AnomaliesPrompt,"account-analysis"=>s.AccountAnalysisPrompt,"trial-balance"=>s.TrialBalancePrompt,"income-statement"=>s.IncomeStatementPrompt,"balance-sheet"=>s.BalanceSheetPrompt,_=>"Interpreta os dados fornecidos sem inventar informação."};
}
