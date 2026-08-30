using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Models;
using TabulariusAI.Web.Services.AI;

namespace TabulariusAI.Web.Controllers;

public sealed class AiAssistantController(TabulariusDbContext dbContext,IAiAssistantService assistant) : Controller
{
    [HttpGet] public async Task<IActionResult> Index(int dossierId,CancellationToken cancellationToken){var dossier=await dbContext.AnalysisDossiers.AsNoTracking().SingleOrDefaultAsync(item=>item.Id==dossierId,cancellationToken);if(dossier is null)return NotFound();return View(new AiChatViewModel{DossierId=dossier.Id,DossierName=dossier.Name});}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Ask(AiChatViewModel model,CancellationToken cancellationToken){var dossier=await dbContext.AnalysisDossiers.AsNoTracking().SingleOrDefaultAsync(item=>item.Id==model.DossierId,cancellationToken);if(dossier is null)return NotFound();model.DossierName=dossier.Name;if(!ModelState.IsValid)return View("Index",model);try{model.Answer=await assistant.AskAsync(model.DossierId,model.Question,cancellationToken);}catch(Exception exception){ModelState.AddModelError(string.Empty,$"Não foi possível obter resposta do modelo: {exception.Message}");}return View("Index",model);}
}
