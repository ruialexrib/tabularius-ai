using TabulariusAI.Web.Data.Entities;

namespace TabulariusAI.Web.Models;

public sealed class SaftPaymentDetailViewModel
{
    public required SaftImportSelectionViewModel Source { get; init; }
    public required SaftPayment Payment { get; init; }
}
