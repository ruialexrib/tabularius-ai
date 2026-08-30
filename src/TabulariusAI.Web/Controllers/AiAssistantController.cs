using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Models;
using TabulariusAI.Web.Services.AI;

namespace TabulariusAI.Web.Controllers;

public sealed class AiAssistantController(TabulariusDbContext dbContext,IAiAssistantService assistant,ILogger<AiAssistantController> logger) : Controller
{
    [HttpGet] public async Task<IActionResult> Index(int dossierId,CancellationToken cancellationToken){var dossier=await dbContext.AnalysisDossiers.AsNoTracking().SingleOrDefaultAsync(item=>item.Id==dossierId,cancellationToken);if(dossier is null)return NotFound();return View(new AiChatViewModel{DossierId=dossier.Id,DossierName=dossier.Name});}

    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Ask([FromBody] AiChatRequest request,CancellationToken cancellationToken)
    {
        if(!ModelState.IsValid)return BadRequest(new{error="Introduza uma pergunta válida."});
        var dossierExists=await dbContext.AnalysisDossiers.AsNoTracking().AnyAsync(item=>item.Id==request.DossierId,cancellationToken);
        if(!dossierExists)return NotFound(new{error="O dossier selecionado não existe."});
        try
        {
            var history=request.History.Where(item=>item.Role is "user" or "assistant"&&!string.IsNullOrWhiteSpace(item.Content)).TakeLast(12).Select(item=>new AiMessage(item.Role,item.Content)).ToArray();
            var answer=await assistant.AskAsync(request.DossierId,request.Question,history,cancellationToken);
            return Json(new{answer});
        }
        catch(OperationCanceledException) when(!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("AI request timed out for dossier {DossierId}.",request.DossierId);
            return StatusCode(StatusCodes.Status504GatewayTimeout,new{error="O modelo demorou demasiado tempo a responder."});
        }
        catch(Exception exception)
        {
            logger.LogError(exception,"AI request failed for dossier {DossierId}.",request.DossierId);
            return StatusCode(StatusCodes.Status502BadGateway,new{error="Não foi possível obter uma resposta do modelo. Verifique a configuração e tente novamente."});
        }
    }
}
