using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Controllers;

public sealed partial class DossierController
{
    /// <summary>Lists movement of goods documents from the selected SAF-T source.</summary>
    public async Task<IActionResult> StockMovements(int id, int? importId, string? search, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var source=await LoadSourceAsync(id,importId,cancellationToken); if(source is null) return NotFound();
        var query=dbContext.SaftStockMovements.AsNoTracking().Where(item=>item.SaftImportId==source.SelectedImport.Id);
        if(!string.IsNullOrWhiteSpace(search)){var term=search.Trim(); query=query.Where(item=>item.DocumentNumber.Contains(term)||item.MovementType.Contains(term)||(item.CustomerId!=null&&item.CustomerId.Contains(term))||(item.SupplierId!=null&&item.SupplierId.Contains(term))||item.SourceId.Contains(term));}
        return View(new SaftListViewModel<SaftStockMovement>{Source=source,List=await PageAsync(query.OrderByDescending(item=>item.MovementDate).ThenBy(item=>item.DocumentNumber),search,page,pageSize,cancellationToken)});
    }

    /// <summary>Shows one movement of goods document and its lines.</summary>
    public async Task<IActionResult> StockMovement(int id, int importId, int movementId, CancellationToken cancellationToken = default)
    {
        var source=await LoadSourceAsync(id,importId,cancellationToken); if(source is null) return NotFound();
        var movement=await dbContext.SaftStockMovements.AsNoTracking().Include(item=>item.Lines).SingleOrDefaultAsync(item=>item.Id==movementId&&item.SaftImportId==source.SelectedImport.Id,cancellationToken);
        return movement is null?NotFound():View(new SaftStockMovementDetailViewModel{Source=source,Movement=movement});
    }
}
