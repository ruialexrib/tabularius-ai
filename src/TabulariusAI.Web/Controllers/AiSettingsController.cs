using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Data.Identity;
using TabulariusAI.Web.Models;
using TabulariusAI.Web.Services.AI;

namespace TabulariusAI.Web.Controllers;

[Authorize(Roles=ApplicationRoles.Administrator)]
public sealed class AiSettingsController(TabulariusDbContext dbContext,IEnumerable<IAiProvider> providers,ILogger<AiSettingsController> logger) : Controller
{
    [HttpGet] public async Task<IActionResult> Index(CancellationToken cancellationToken){var settings=await dbContext.AiSettings.AsNoTracking().SingleOrDefaultAsync(cancellationToken)??new AiSettings();return View(new AiSettingsViewModel{IsEnabled=settings.IsEnabled,Provider=settings.Provider,Endpoint=settings.Endpoint,Model=settings.Model,Temperature=settings.Temperature,TimeoutSeconds=settings.TimeoutSeconds,SystemPrompt=settings.SystemPrompt,HomePrompt=settings.HomePrompt,HomeCacheMinutes=settings.HomeCacheMinutes,AnalyticsOverviewPrompt=settings.AnalyticsOverviewPrompt,AnomaliesPrompt=settings.AnomaliesPrompt,AccountAnalysisPrompt=settings.AccountAnalysisPrompt,TrialBalancePrompt=settings.TrialBalancePrompt,IncomeStatementPrompt=settings.IncomeStatementPrompt,BalanceSheetPrompt=settings.BalanceSheetPrompt,HasApiKey=!string.IsNullOrWhiteSpace(settings.ApiKey)});}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Index(AiSettingsViewModel settingsModel,CancellationToken cancellationToken)
    {
        if(settingsModel.Provider is not ("Ollama" or "Mistral")) ModelState.AddModelError(nameof(settingsModel.Provider),"Selecione Ollama ou Mistral."); if(!ModelState.IsValid) return View(settingsModel);
        var settings=await dbContext.AiSettings.SingleOrDefaultAsync(cancellationToken); if(settings is null){settings=new AiSettings();dbContext.AiSettings.Add(settings);} settings.IsEnabled=settingsModel.IsEnabled;settings.Provider=settingsModel.Provider;settings.Endpoint=settingsModel.Endpoint.Trim();settings.Model=settingsModel.Model.Trim();settings.Temperature=settingsModel.Temperature;settings.TimeoutSeconds=settingsModel.TimeoutSeconds;settings.SystemPrompt=settingsModel.SystemPrompt.Trim();settings.HomePrompt=settingsModel.HomePrompt.Trim();settings.HomeCacheMinutes=settingsModel.HomeCacheMinutes;settings.AnalyticsOverviewPrompt=settingsModel.AnalyticsOverviewPrompt.Trim();settings.AnomaliesPrompt=settingsModel.AnomaliesPrompt.Trim();settings.AccountAnalysisPrompt=settingsModel.AccountAnalysisPrompt.Trim();settings.TrialBalancePrompt=settingsModel.TrialBalancePrompt.Trim();settings.IncomeStatementPrompt=settingsModel.IncomeStatementPrompt.Trim();settings.BalanceSheetPrompt=settingsModel.BalanceSheetPrompt.Trim();if(!string.IsNullOrWhiteSpace(settingsModel.ApiKey))settings.ApiKey=settingsModel.ApiKey.Trim();settings.UpdatedAtUtc=DateTime.UtcNow;await dbContext.SaveChangesAsync(cancellationToken);TempData["AiSettingsSaved"]="Definições de inteligência artificial guardadas.";return RedirectToAction(nameof(Index));
    }

    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> TestConnection(CancellationToken cancellationToken)
    {
        var settings=await dbContext.AiSettings.AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        if(settings is null){TempData["AiSettingsError"]="Guarde primeiro as definições do modelo.";return RedirectToAction(nameof(Index));}
        var provider=providers.SingleOrDefault(item=>string.Equals(item.Name,settings.Provider,StringComparison.OrdinalIgnoreCase));
        if(provider is null){TempData["AiSettingsError"]=$"O fornecedor '{settings.Provider}' não é suportado.";return RedirectToAction(nameof(Index));}
        if(string.Equals(settings.Provider,"Mistral",StringComparison.OrdinalIgnoreCase)&&string.IsNullOrWhiteSpace(settings.ApiKey)){TempData["AiSettingsError"]="Configure a API key da Mistral antes de testar a ligação.";return RedirectToAction(nameof(Index));}
        try
        {
            using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(Math.Clamp(settings.TimeoutSeconds,10,600)));
            using var linked=CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,timeout.Token);
            var compatibilityTool=new AiToolDefinition("tabularius_compatibility_check","Tool de diagnóstico usada para confirmar que o modelo suporta tool calling.",new { type="object",properties=new { },required=Array.Empty<string>(),additionalProperties=false });
            var result=await provider.CompleteAsync(settings.Endpoint,settings.Model,settings.ApiKey,0,"Estás a executar um teste técnico de compatibilidade. Tens de chamar obrigatoriamente a tool tabularius_compatibility_check. Não respondas diretamente ao utilizador.",[new AiMessage("user","Chama agora a tool tabularius_compatibility_check para validar a compatibilidade com o Tabularius AI.")],[compatibilityTool],linked.Token);
            if(!result.ToolCalls.Any(call=>string.Equals(call.Name,"tabularius_compatibility_check",StringComparison.Ordinal))){TempData["AiSettingsError"]=$"O modelo {settings.Model} responde através de {settings.Provider}, mas não confirmou suporte a tool calling. Esta funcionalidade é necessária para consultar os dados do dossier.";return RedirectToAction(nameof(Index));}
            TempData["AiSettingsTest"]=$"Compatibilidade confirmada · {settings.Provider} · {settings.Model} · tool calling disponível";
        }
        catch(HttpRequestException exception) when(exception.Message.Contains("does not support tools",StringComparison.OrdinalIgnoreCase)||exception.Message.Contains("tool",StringComparison.OrdinalIgnoreCase)&&exception.StatusCode==System.Net.HttpStatusCode.BadRequest){logger.LogWarning(exception,"AI model {Model} on provider {Provider} does not support required tool calling.",settings.Model,settings.Provider);TempData["AiSettingsError"]=$"O modelo {settings.Model} responde através de {settings.Provider}, mas não suporta tool calling. Esta funcionalidade é necessária para consultar os dados do dossier.";}
        catch(Exception exception) when(exception is HttpRequestException or InvalidOperationException or TaskCanceledException){logger.LogWarning(exception,"AI compatibility test failed for provider {Provider} and model {Model}.",settings.Provider,settings.Model);TempData["AiSettingsError"]=$"Não foi possível confirmar a compatibilidade de {settings.Provider} com o modelo {settings.Model}. Verifique o endpoint, autenticação e disponibilidade do modelo.";}
        return RedirectToAction(nameof(Index));
    }
}
