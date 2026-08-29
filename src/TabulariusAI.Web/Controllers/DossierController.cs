using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Data.Entities;

namespace TabulariusAI.Web.Controllers;

/// <summary>
/// Provides read-only navigation over accounting entities and their analysis dossiers.
/// </summary>
public sealed class DossierController(TabulariusDbContext dbContext) : Controller
{
    /// <summary>Displays the accounting entities currently available in the local workspace.</summary>
    /// <param name="cancellationToken">A token used to cancel the database operation.</param>
    /// <returns>The entities view.</returns>
    public async Task<IActionResult> Entities(CancellationToken cancellationToken) => View(await dbContext.AccountingEntities.AsNoTracking().Include(item => item.Dossiers).OrderBy(item => item.Name).ToListAsync(cancellationToken));

    /// <summary>Displays one accounting entity and the dossiers available for it.</summary>
    /// <param name="id">The accounting entity identifier.</param><param name="cancellationToken">A cancellation token.</param>
    /// <returns>The entity workspace view, or a not-found result.</returns>
    public async Task<IActionResult> Entity(int id, CancellationToken cancellationToken) { var entity = await dbContext.AccountingEntities.AsNoTracking().Include(item => item.Dossiers).ThenInclude(item => item.Imports).SingleOrDefaultAsync(item => item.Id == id, cancellationToken); return entity is null ? NotFound() : View(entity); }

    /// <summary>Displays one analysis dossier including its SAF-T (PT) import history.</summary>
    /// <param name="id">The dossier identifier.</param><param name="cancellationToken">A cancellation token.</param>
    /// <returns>The dossier workspace view, or a not-found result.</returns>
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken) { var dossier = await LoadDossierAsync(id, cancellationToken); return dossier is null ? NotFound() : View(dossier); }

    /// <summary>Displays the SAF-T (PT) source summary for a selected accounting dossier.</summary>
    /// <param name="id">The dossier identifier.</param><param name="cancellationToken">A cancellation token.</param>
    /// <returns>The SAF-T summary view.</returns>
    public async Task<IActionResult> SaftSummary(int id, CancellationToken cancellationToken) { var dossier = await LoadDossierAsync(id, cancellationToken); return dossier is null ? NotFound() : View(dossier); }

    /// <summary>Displays the chart of accounts from the most recent SAF-T (PT) import.</summary>
    /// <param name="id">The dossier identifier.</param><param name="cancellationToken">A cancellation token.</param>
    /// <returns>The account list.</returns>
    public async Task<IActionResult> Accounts(int id, CancellationToken cancellationToken) => await LatestImportViewAsync(id, cancellationToken, query => query.Include(item => item.Accounts));

    /// <summary>Displays customers from the most recent SAF-T (PT) import.</summary>
    /// <param name="id">The dossier identifier.</param><param name="cancellationToken">A cancellation token.</param>
    /// <returns>The customer list.</returns>
    public async Task<IActionResult> Customers(int id, CancellationToken cancellationToken) => await LatestImportViewAsync(id, cancellationToken, query => query.Include(item => item.Customers));

    /// <summary>Displays suppliers from the most recent SAF-T (PT) import.</summary>
    /// <param name="id">The dossier identifier.</param><param name="cancellationToken">A cancellation token.</param>
    /// <returns>The supplier list.</returns>
    public async Task<IActionResult> Suppliers(int id, CancellationToken cancellationToken) => await LatestImportViewAsync(id, cancellationToken, query => query.Include(item => item.Suppliers));

    /// <summary>Loads the latest import and applies the requested master-data include.</summary>
    /// <param name="id">The dossier identifier.</param><param name="cancellationToken">A cancellation token.</param><param name="include">The include operation.</param>
    /// <returns>The current action view or a not-found result.</returns>
    private async Task<IActionResult> LatestImportViewAsync(int id, CancellationToken cancellationToken, Func<IQueryable<SaftImport>, IQueryable<SaftImport>> include)
    {
        IQueryable<SaftImport> query = dbContext.SaftImports.AsNoTracking().Include(item => item.Dossier).ThenInclude(item => item.AccountingEntity);
        var import = await include(query).Where(item => item.DossierId == id).OrderByDescending(item => item.ImportedAtUtc).FirstOrDefaultAsync(cancellationToken);
        return import is null ? NotFound() : View(import);
    }

    /// <summary>Loads a dossier with the entity and SAF-T (PT) imports required by workspace views.</summary>
    /// <param name="id">The dossier identifier.</param><param name="cancellationToken">A cancellation token.</param>
    /// <returns>The requested dossier, or <see langword="null"/> when it does not exist.</returns>
    private async Task<AnalysisDossier?> LoadDossierAsync(int id, CancellationToken cancellationToken) => await dbContext.AnalysisDossiers.AsNoTracking().Include(item => item.AccountingEntity).Include(item => item.Imports).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
}
