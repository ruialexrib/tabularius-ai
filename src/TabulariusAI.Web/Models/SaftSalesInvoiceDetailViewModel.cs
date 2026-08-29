using TabulariusAI.Web.Data.Entities;

namespace TabulariusAI.Web.Models;

public sealed class SaftSalesInvoiceDetailViewModel
{
    public SaftImportSelectionViewModel Source { get; set; } = null!;
    public SaftSalesInvoice Invoice { get; set; } = null!;
}
