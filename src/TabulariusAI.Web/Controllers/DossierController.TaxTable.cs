using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Controllers;

public sealed partial class DossierController
{
    public async Task<IActionResult> TaxTable(int id, int? importId, string? search, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var source = await LoadSourceAsync(id, importId, cancellationToken);
        if (source is null) return NotFound();
        var query = dbContext.SaftTaxEntries.AsNoTracking().Where(item => item.SaftImportId == source.SelectedImport.Id);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(item => item.TaxType.Contains(term) || item.TaxCountryRegion.Contains(term) || item.TaxCode.Contains(term) || item.Description.Contains(term));
        }
        return View(new SaftListViewModel<SaftTaxEntry>
        {
            Source = source,
            List = await PageAsync(query.OrderBy(item => item.TaxType).ThenBy(item => item.TaxCountryRegion).ThenBy(item => item.TaxCode), search, page, pageSize, cancellationToken)
        });
    }
}
