namespace TabulariusAI.Web.Data.Entities;

/// <summary>
/// Represents an accounting analysis dossier for a specific entity and fiscal period.
/// </summary>
public sealed class AnalysisDossier
{
    /// <summary>Gets or sets the dossier identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the accounting entity identifier.</summary>
    public int AccountingEntityId { get; set; }

    /// <summary>Gets or sets the dossier display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the fiscal year represented by the dossier.</summary>
    public int FiscalYear { get; set; }

    /// <summary>Gets or sets the UTC date and time when the dossier was created.</summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the accounting entity that owns the dossier.</summary>
    public AccountingEntity AccountingEntity { get; set; } = null!;

    /// <summary>Gets the SAF-T (PT) imports associated with the dossier.</summary>
    public ICollection<SaftImport> Imports { get; } = new List<SaftImport>();
}
