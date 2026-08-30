using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Data.Identity;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Controllers;

[Authorize(Roles=ApplicationRoles.Administrator)]
public sealed class AiSettingsController(TabulariusDbContext dbContext) : Controller
{
    [HttpGet] public async Task<IActionResult> Index(CancellationToken cancellationToken){var settings=await dbContext.AiSettings.AsNoTracking().SingleOrDefaultAsync(cancellationToken)??new AiSettings();return View(new AiSettingsViewModel{IsEnabled=settings.IsEnabled,Provider=settings.Provider,Endpoint=settings.Endpoint,Model=settings.Model,Temperature=settings.Temperature,TimeoutSeconds=settings.TimeoutSeconds,SystemPrompt=settings.SystemPrompt,HasApiKey=!string.IsNullOrWhiteSpace(settings.ApiKey)});}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Index(AiSettingsViewModel settingsModel,CancellationToken cancellationToken)
    {
        if(settingsModel.Provider is not ("Ollama" or "Mistral")) ModelState.AddModelError(nameof(settingsModel.Provider),"Selecione Ollama ou Mistral."); if(!ModelState.IsValid) return View(settingsModel);
        var settings=await dbContext.AiSettings.SingleOrDefaultAsync(cancellationToken); if(settings is null){settings=new AiSettings();dbContext.AiSettings.Add(settings);} settings.IsEnabled=settingsModel.IsEnabled;settings.Provider=settingsModel.Provider;settings.Endpoint=settingsModel.Endpoint.Trim();settings.Model=settingsModel.Model.Trim();settings.Temperature=settingsModel.Temperature;settings.TimeoutSeconds=settingsModel.TimeoutSeconds;settings.SystemPrompt=settingsModel.SystemPrompt.Trim();if(!string.IsNullOrWhiteSpace(settingsModel.ApiKey))settings.ApiKey=settingsModel.ApiKey.Trim();settings.UpdatedAtUtc=DateTime.UtcNow;await dbContext.SaveChangesAsync(cancellationToken);TempData["AiSettingsSaved"]="Definições de inteligência artificial guardadas.";return RedirectToAction(nameof(Index));
    }
}
