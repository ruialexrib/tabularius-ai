using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data;

namespace TabulariusAI.Web.Controllers;

/// <summary>
/// Provides read-only navigation over accounting entities, analysis dossiers and SAF-T (PT) imports.
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
    /// Displays the analysis dossiers currently available in the local workspace.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the database operation.</param>
    /// <returns>The dossiers view.</returns>
    public async Task<IActionResult> Dossiers(CancellationToken cancellationToken)
    {
        var dossiers = await dbContext.AnalysisDossiers.AsNoTracking()
            .Include(item => item.AccountingEntity)
            .Include(item => item.Imports)
            .OrderByDescending(item => item.FiscalYear)
            .ThenBy(item => item.AccountingEntity.Name)
            .ToListAsync(cancellationToken);
        return View(dossiers);
    }

    /// <summary>
    /// Displays the SAF-T (PT) imports currently available in the local workspace.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the database operation.</param>
    /// <returns>The imports view.</returns>
    public async Task<IActionResult> Imports(CancellationToken cancellationToken)
    {
        var imports = await dbContext.SaftImports.AsNoTracking()
            .Include(item => item.Dossier)
            .ThenInclude(item => item.AccountingEntity)
            .OrderByDescending(item => item.ImportedAtUtc)
            .ToListAsync(cancellationToken);
        return View(imports);
    }
}
