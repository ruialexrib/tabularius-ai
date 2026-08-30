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
    [HttpGet] public async Task<IActionResult> Index(CancellationToken cancellationToken){var settings=await dbContext.AiSettings.AsNoTracking().SingleOrDefaultAsync(cancellationToken)??new AiSettings();return View(new AiSettingsViewModel{IsEnabled=settings.IsEnabled,Provider=settings.Provider,Endpoint=settings.Endpoint,Model=settings.Model,Temperature=settings.Temperature,TimeoutSeconds=settings.TimeoutSeconds,SystemPrompt=settings.SystemPrompt,HasApiKey=!string.IsNullOrWhiteSpace(settings.ApiKey)});}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Index(AiSettingsViewModel settingsModel,CancellationToken cancellationToken)
    {
        if(settingsModel.Provider is not ("Ollama" or "Mistral")) ModelState.AddModelError(nameof(settingsModel.Provider),"Selecione Ollama ou Mistral."); if(!ModelState.IsValid) return View(settingsModel);
        var settings=await dbContext.AiSettings.SingleOrDefaultAsync(cancellationToken); if(settings is null){settings=new AiSettings();dbContext.AiSettings.Add(settings);} settings.IsEnabled=settingsModel.IsEnabled;settings.Provider=settingsModel.Provider;settings.Endpoint=settingsModel.Endpoint.Trim();settings.Model=settingsModel.Model.Trim();settings.Temperature=settingsModel.Temperature;settings.TimeoutSeconds=settingsModel.TimeoutSeconds;settings.SystemPrompt=settingsModel.SystemPrompt.Trim();if(!string.IsNullOrWhiteSpace(settingsModel.ApiKey))settings.ApiKey=settingsModel.ApiKey.Trim();settings.UpdatedAtUtc=DateTime.UtcNow;await dbContext.SaveChangesAsync(cancellationToken);TempData["AiSettingsSaved"]="Definições de inteligência artificial guardadas.";return RedirectToAction(nameof(Index));
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
            var result=await provider.CompleteAsync(settings.Endpoint,settings.Model,settings.ApiKey,0,"Responde apenas com OK.",[new AiMessage("user","Responde apenas com OK para confirmar que o modelo está disponível.")],[],linked.Token);
            if(string.IsNullOrWhiteSpace(result.Content)) throw new InvalidOperationException("O modelo não devolveu conteúdo.");
            TempData["AiSettingsTest"]=$"Ligação confirmada · {settings.Provider} · {settings.Model}";
        }
        catch(Exception exception) when(exception is HttpRequestException or InvalidOperationException or TaskCanceledException)
        {
            logger.LogWarning(exception,"AI connection test failed for provider {Provider} and model {Model}.",settings.Provider,settings.Model);
            TempData["AiSettingsError"]=$"Não foi possível confirmar a ligação a {settings.Provider} com o modelo {settings.Model}.";
        }
        return RedirectToAction(nameof(Index));
    }
}
