using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Controllers;

public sealed partial class DossierController
{
    public async Task<IActionResult> SalesInvoices(int id, int? importId, string? search, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var source = await LoadSourceAsync(id, importId, cancellationToken); if (source is null) return NotFound();
        var query = dbContext.SaftSalesInvoices.AsNoTracking().Where(item => item.SaftImportId == source.SelectedImport.Id);
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim(); query = query.Where(item => item.InvoiceNo.Contains(term) || item.InvoiceType.Contains(term) || (item.CustomerId != null && item.CustomerId.Contains(term)) || item.SourceId.Contains(term)); }
        return View(new SaftListViewModel<SaftSalesInvoice> { Source = source, List = await PageAsync(query.OrderByDescending(item => item.InvoiceDate).ThenBy(item => item.InvoiceNo), search, page, pageSize, cancellationToken) });
    }

    public async Task<IActionResult> SalesInvoice(int id, int importId, int invoiceId, CancellationToken cancellationToken = default)
    {
        var source = await LoadSourceAsync(id, importId, cancellationToken); if (source is null) return NotFound();
        var invoice = await dbContext.SaftSalesInvoices.AsNoTracking().Include(item => item.Lines).SingleOrDefaultAsync(item => item.Id == invoiceId && item.SaftImportId == source.SelectedImport.Id, cancellationToken);
        return invoice is null ? NotFound() : View(new SaftSalesInvoiceDetailViewModel { Source = source, Invoice = invoice });
    }
}
