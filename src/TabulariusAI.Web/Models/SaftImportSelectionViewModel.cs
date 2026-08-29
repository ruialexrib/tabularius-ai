using TabulariusAI.Web.Data.Entities;

namespace TabulariusAI.Web.Models;

/// <summary>
/// Provides a selected SAF-T (PT) source together with all sources available in the same dossier.
/// </summary>
public sealed class SaftImportSelectionViewModel
{
    /// <summary>Gets or sets the selected SAF-T (PT) import.</summary>
    public SaftImport SelectedImport { get; set; } = null!;

    /// <summary>Gets or sets the SAF-T (PT) imports available for source selection.</summary>
    public IReadOnlyList<SaftImport> AvailableImports { get; set; } = [];
}
