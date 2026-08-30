using TabulariusAI.Web.Data.Entities;
namespace TabulariusAI.Web.Models;
public sealed class SaftWorkingDocumentDetailViewModel
{
    public SaftImportSelectionViewModel Source { get; init; } = null!;
    public SaftWorkingDocument Document { get; init; } = null!;
}
