using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data;

namespace TabulariusAI.Web.Controllers;

/// <summary>
/// Provides read-only navigation over accounting entities and their analysis dossiers.
/// </summary>
public sealed class DossierController(TabulariusDbContext dbContext) : Controller
{
    /// <summary>
    /// Displays the accounting entities currently available in the local workspace.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the database operation.</param>
    /// <returns>The entities view.</returns>
    public async Task<IActionResult> Entities(CancellationToken cancellationToken)
    {
        var entities = await dbContext.AccountingEntities.AsNoTracking()
            .Include(item => item.Dossiers)
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);
        return View(entities);
    }

    /// <summary>
    /// Displays one accounting entity and the dossiers available for it.
    /// </summary>
    /// <param name="id">The accounting entity identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the database operation.</param>
    /// <returns>The entity workspace view, or a not-found result.</returns>
    public async Task<IActionResult> Entity(int id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.AccountingEntities.AsNoTracking()
            .Include(item => item.Dossiers)
            .ThenInclude(item => item.Imports)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        return entity is null ? NotFound() : View(entity);
    }

    /// <summary>
    /// Displays one analysis dossier including its SAF-T (PT) import history.
    /// </summary>
    /// <param name="id">The analysis dossier identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the database operation.</param>
    /// <returns>The dossier workspace view, or a not-found result.</returns>
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var dossier = await dbContext.AnalysisDossiers.AsNoTracking()
            .Include(item => item.AccountingEntity)
            .Include(item => item.Imports)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        return dossier is null ? NotFound() : View(dossier);
    }
}
