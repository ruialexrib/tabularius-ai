using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Controllers;

public sealed partial class DossierController
{
    public async Task<IActionResult> Payments(int id,int? importId,string? search,int page=1,int pageSize=10,CancellationToken ct=default)
    {
        var source=await LoadSourceAsync(id,importId,ct);if(source is null)return NotFound();
        var query=dbContext.Set<SaftPayment>().AsNoTracking().Where(x=>x.SaftImportId==source.SelectedImport.Id);
        if(!string.IsNullOrWhiteSpace(search)){var term=search.Trim();query=query.Where(x=>x.PaymentRefNo.Contains(term)||x.PaymentType.Contains(term)||x.SourceId.Contains(term)||(x.CustomerId!=null&&x.CustomerId.Contains(term)));}
        return View(new SaftListViewModel<SaftPayment>{Source=source,List=await PageAsync(query.OrderByDescending(x=>x.TransactionDate).ThenBy(x=>x.PaymentRefNo),search,page,pageSize,ct)});
    }

    public async Task<IActionResult> Payment(int id,int importId,int paymentId,CancellationToken ct=default)
    {
        var source=await LoadSourceAsync(id,importId,ct);if(source is null)return NotFound();
        var payment=await dbContext.Set<SaftPayment>().AsNoTracking().Include(x=>x.Lines).SingleOrDefaultAsync(x=>x.Id==paymentId&&x.SaftImportId==source.SelectedImport.Id,ct);
        return payment is null?NotFound():View(new SaftPaymentDetailViewModel{Source=source,Payment=payment});
    }
}
