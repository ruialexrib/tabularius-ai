using TabulariusAI.Web.Data.Entities;

namespace TabulariusAI.Web.Models;

/// <summary>Represents one accounting entity together with its filtered and paginated dossier list.</summary>
public sealed class EntityDossiersViewModel
{
    /// <summary>Gets or sets the accounting entity displayed by the page.</summary>
    public AccountingEntity Entity { get; set; } = null!;

    /// <summary>Gets or sets the filtered and paginated accounting dossiers.</summary>
    public PagedListViewModel<AnalysisDossier> List { get; set; } = new();
}
