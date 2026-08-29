using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Controllers;

/// <summary>
/// Provides navigation and data management operations over accounting entities and their analysis dossiers.
/// </summary>
public sealed class DossierController(TabulariusDbContext dbContext) : Controller
{
    private static readonly int[] AllowedPageSizes = [10, 25, 50, 100];

    /// <summary>Displays a filtered and paginated list of accounting entities.</summary>
    /// <param name="search">Optional entity name or tax identifier search.</param><param name="page">The requested one-based page.</param><param name="pageSize">The requested page size.</param><param name="cancellationToken">A cancellation token.</param>
    /// <returns>The entities list view.</returns>
    public async Task<IActionResult> Entities(string? search, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var query = dbContext.AccountingEntities.AsNoTracking().Include(item => item.Dossiers).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(item => item.Name.Contains(term) || item.TaxRegistrationNumber.Contains(term));
        }
        return View(await PageAsync(query.OrderBy(item => item.Name), search, page, pageSize, cancellationToken));
    }

    /// <summary>Displays one accounting entity and the dossiers available for it.</summary>
    /// <param name="id">The accounting entity identifier.</param><param name="cancellationToken">A cancellation token.</param>
    /// <returns>The entity workspace view, or a not-found result.</returns>
    public async Task<IActionResult> Entity(int id, CancellationToken cancellationToken) { var entity = await dbContext.AccountingEntities.AsNoTracking().Include(item => item.Dossiers).ThenInclude(item => item.Imports).SingleOrDefaultAsync(item => item.Id == id, cancellationToken); return entity is null ? NotFound() : View(entity); }

    /// <summary>Displays one analysis dossier including its SAF-T (PT) import history.</summary>
    /// <param name="id">The dossier identifier.</param><param name="cancellationToken">A cancellation token.</param>
    /// <returns>The dossier workspace view, or a not-found result.</returns>
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken) { var dossier = await LoadDossierAsync(id, cancellationToken); return dossier is null ? NotFound() : View(dossier); }

    /// <summary>Deletes an accounting entity and all dossiers and imported data owned by it.</summary>
    /// <param name="id">The accounting entity identifier.</param><param name="cancellationToken">A cancellation token.</param>
    /// <returns>A redirect to the entities workspace.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEntity(int id, CancellationToken cancellationToken) { var entity = await dbContext.AccountingEntities.SingleOrDefaultAsync(item => item.Id == id, cancellationToken); if (entity is null) return NotFound(); dbContext.AccountingEntities.Remove(entity); await dbContext.SaveChangesAsync(cancellationToken); TempData["SuccessMessage"] = "A entidade e todos os respetivos dossiers e dados importados foram eliminados."; return RedirectToAction(nameof(Entities)); }

    /// <summary>Deletes an analysis dossier and all imported data owned by it.</summary>
    /// <param name="id">The dossier identifier.</param><param name="cancellationToken">A cancellation token.</param>
    /// <returns>A redirect to the owning entity workspace.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDossier(int id, CancellationToken cancellationToken) { var dossier = await dbContext.AnalysisDossiers.SingleOrDefaultAsync(item => item.Id == id, cancellationToken); if (dossier is null) return NotFound(); var entityId = dossier.AccountingEntityId; dbContext.AnalysisDossiers.Remove(dossier); await dbContext.SaveChangesAsync(cancellationToken); TempData["SuccessMessage"] = "O dossier e todas as respetivas importações foram eliminados."; return RedirectToAction(nameof(Entity), new { id = entityId }); }

    /// <summary>Deletes one SAF-T (PT) import and all source data owned by it.</summary>
    /// <param name="id">The SAF-T (PT) import identifier.</param><param name="cancellationToken">A cancellation token.</param>
    /// <returns>A redirect to the owning dossier workspace.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImport(int id, CancellationToken cancellationToken) { var import = await dbContext.SaftImports.SingleOrDefaultAsync(item => item.Id == id, cancellationToken); if (import is null) return NotFound(); var dossierId = import.DossierId; dbContext.SaftImports.Remove(import); await dbContext.SaveChangesAsync(cancellationToken); TempData["SuccessMessage"] = "A importação SAF-T (PT) e os respetivos dados foram eliminados."; return RedirectToAction(nameof(Details), new { id = dossierId }); }

    /// <summary>Displays the summary for a selected SAF-T (PT) source.</summary>
    /// <param name="id">The dossier identifier.</param><param name="importId">The optional SAF-T (PT) import identifier.</param><param name="cancellationToken">A cancellation token.</param>
    /// <returns>The selected SAF-T summary.</returns>
    public async Task<IActionResult> SaftSummary(int id, int? importId, CancellationToken cancellationToken) => await ImportViewAsync(id, importId, cancellationToken, query => query.Include(item => item.Accounts).Include(item => item.Customers).Include(item => item.Suppliers));

    /// <summary>Displays a filtered and paginated chart of accounts from a selected SAF-T (PT) import.</summary>
    public async Task<IActionResult> Accounts(int id, int? importId, string? search, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var source = await LoadSourceAsync(id, importId, cancellationToken); if (source is null) return NotFound();
        var query = dbContext.SaftAccounts.AsNoTracking().Where(item => item.SaftImportId == source.SelectedImport.Id);
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim(); query = query.Where(item => item.AccountId.Contains(term) || item.Description.Contains(term) || (item.TaxonomyReference != null && item.TaxonomyReference.Contains(term))); }
        return View(new SaftListViewModel<SaftAccount> { Source = source, List = await PageAsync(query.OrderBy(item => item.AccountId), search, page, pageSize, cancellationToken) });
    }

    /// <summary>Displays a filtered and paginated customer list from a selected SAF-T (PT) import.</summary>
    public async Task<IActionResult> Customers(int id, int? importId, string? search, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var source = await LoadSourceAsync(id, importId, cancellationToken); if (source is null) return NotFound();
        var query = dbContext.SaftCustomers.AsNoTracking().Where(item => item.SaftImportId == source.SelectedImport.Id);
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim(); query = query.Where(item => item.CustomerId.Contains(term) || item.CompanyName.Contains(term) || item.TaxId.Contains(term) || item.AccountId.Contains(term)); }
        return View(new SaftListViewModel<SaftCustomer> { Source = source, List = await PageAsync(query.OrderBy(item => item.CompanyName).ThenBy(item => item.CustomerId), search, page, pageSize, cancellationToken) });
    }

    /// <summary>Displays a filtered and paginated supplier list from a selected SAF-T (PT) import.</summary>
    public async Task<IActionResult> Suppliers(int id, int? importId, string? search, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var source = await LoadSourceAsync(id, importId, cancellationToken); if (source is null) return NotFound();
        var query = dbContext.SaftSuppliers.AsNoTracking().Where(item => item.SaftImportId == source.SelectedImport.Id);
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim(); query = query.Where(item => item.SupplierId.Contains(term) || item.CompanyName.Contains(term) || item.TaxId.Contains(term) || item.AccountId.Contains(term)); }
        return View(new SaftListViewModel<SaftSupplier> { Source = source, List = await PageAsync(query.OrderBy(item => item.CompanyName).ThenBy(item => item.SupplierId), search, page, pageSize, cancellationToken) });
    }

    /// <summary>Loads a selected import, defaulting to the latest accounting period, and exposes all dossier sources.</summary>
    private async Task<IActionResult> ImportViewAsync(int id, int? importId, CancellationToken cancellationToken, Func<IQueryable<SaftImport>, IQueryable<SaftImport>> include) { var source = await LoadSourceAsync(id, importId, cancellationToken, include); return source is null ? NotFound() : View(source); }

    /// <summary>Loads source-selection context for a dossier without loading list rows.</summary>
    private async Task<SaftImportSelectionViewModel?> LoadSourceAsync(int id, int? importId, CancellationToken cancellationToken, Func<IQueryable<SaftImport>, IQueryable<SaftImport>>? include = null)
    {
        var availableImports = await dbContext.SaftImports.AsNoTracking().Where(item => item.DossierId == id).OrderByDescending(item => item.EndDate).ThenByDescending(item => item.StartDate).ThenByDescending(item => item.ImportedAtUtc).ThenByDescending(item => item.Id).ToListAsync(cancellationToken);
        if (availableImports.Count == 0) return null;
        var selectedId = importId ?? availableImports[0].Id; if (!availableImports.Any(item => item.Id == selectedId)) return null;
        IQueryable<SaftImport> query = dbContext.SaftImports.AsNoTracking().Include(item => item.Dossier).ThenInclude(item => item.AccountingEntity); if (include is not null) query = include(query);
        var selectedImport = await query.SingleOrDefaultAsync(item => item.Id == selectedId && item.DossierId == id, cancellationToken);
        return selectedImport is null ? null : new SaftImportSelectionViewModel { SelectedImport = selectedImport, AvailableImports = availableImports };
    }

    /// <summary>Creates a normalized server-side page from a query.</summary>
    private static async Task<PagedListViewModel<T>> PageAsync<T>(IQueryable<T> query, string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        pageSize = AllowedPageSizes.Contains(pageSize) ? pageSize : 10; page = Math.Max(1, page);
        var total = await query.CountAsync(cancellationToken); var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)); page = Math.Min(page, totalPages);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedListViewModel<T> { Items = items, TotalItems = total, Page = page, PageSize = pageSize, Search = search?.Trim() };
    }

    /// <summary>Loads a dossier with the entity and SAF-T (PT) imports required by workspace views.</summary>
    private async Task<AnalysisDossier?> LoadDossierAsync(int id, CancellationToken cancellationToken) => await dbContext.AnalysisDossiers.AsNoTracking().Include(item => item.AccountingEntity).Include(item => item.Imports).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
}
