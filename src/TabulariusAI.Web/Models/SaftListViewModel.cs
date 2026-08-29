using TabulariusAI.Web.Data.Entities;

namespace TabulariusAI.Web.Models;

/// <summary>
/// Combines SAF-T source selection with a reusable paginated list.
/// </summary>
/// <typeparam name="T">The SAF-T row type displayed by the list.</typeparam>
public sealed class SaftListViewModel<T>
{
    /// <summary>Gets or sets the selected SAF-T source and available dossier sources.</summary>
    public SaftImportSelectionViewModel Source { get; set; } = null!;

    /// <summary>Gets or sets the paginated rows for the selected source.</summary>
    public PagedListViewModel<T> List { get; set; } = null!;
}
