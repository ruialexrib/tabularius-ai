using TabulariusAI.Web.Data.Entities;

namespace TabulariusAI.Web.Models;

/// <summary>Provides a movement of goods document and its selected SAF-T source to the detail view.</summary>
public sealed class SaftStockMovementDetailViewModel
{
    public SaftImportSelectionViewModel Source { get; set; } = null!;
    public SaftStockMovement Movement { get; set; } = null!;
}
