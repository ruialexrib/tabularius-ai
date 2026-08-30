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
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Index(AiSettingsViewModel model,CancellationToken cancellationToken)
    {
        if(model.Provider is not ("Ollama" or "Mistral")) ModelState.AddModelError(nameof(model.Provider),"Selecione Ollama ou Mistral."); if(!ModelState.IsValid) return View(model);
        var settings=await dbContext.AiSettings.SingleOrDefaultAsync(cancellationToken); if(settings is null){settings=new AiSettings();dbContext.AiSettings.Add(settings);} settings.IsEnabled=model.IsEnabled;settings.Provider=model.Provider;settings.Endpoint=model.Endpoint.Trim();settings.Model=model.Model.Trim();settings.Temperature=model.Temperature;settings.TimeoutSeconds=model.TimeoutSeconds;settings.SystemPrompt=model.SystemPrompt.Trim();if(!string.IsNullOrWhiteSpace(model.ApiKey))settings.ApiKey=model.ApiKey.Trim();settings.UpdatedAtUtc=DateTime.UtcNow;await dbContext.SaveChangesAsync(cancellationToken);TempData["AiSettingsSaved"]="Definições de inteligência artificial guardadas.";return RedirectToAction(nameof(Index));
    }
}
