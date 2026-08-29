namespace TabulariusAI.Web.Data.Entities;

/// <summary>
/// Represents metadata about a SAF-T (PT) file imported into an analysis dossier.
/// </summary>
public sealed class SaftImport
{
    /// <summary>Gets or sets the import identifier.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the dossier identifier.</summary>
    public int DossierId { get; set; }
    /// <summary>Gets or sets the original SAF-T (PT) file name.</summary>
    public string OriginalFileName { get; set; } = string.Empty;
    /// <summary>Gets or sets the SHA-256 hash of the exact imported SAF-T (PT) file content.</summary>
    public string? ContentHash { get; set; }
    /// <summary>Gets or sets the SAF-T (PT) version declared by the imported document.</summary>
    public string SaftVersion { get; set; } = string.Empty;
    /// <summary>Gets or sets the start date represented by the imported document.</summary>
    public DateOnly? StartDate { get; set; }
    /// <summary>Gets or sets the end date represented by the imported document.</summary>
    public DateOnly? EndDate { get; set; }
    /// <summary>Gets or sets the UTC date and time when the file was imported.</summary>
    public DateTime ImportedAtUtc { get; set; } = DateTime.UtcNow;
    /// <summary>Gets or sets the dossier that owns the import.</summary>
    public AnalysisDossier Dossier { get; set; } = null!;
    /// <summary>Gets the ledger accounts imported from this exact SAF-T (PT) source.</summary>
    public ICollection<SaftAccount> Accounts { get; } = new List<SaftAccount>();
    /// <summary>Gets the customers imported from this exact SAF-T (PT) source.</summary>
    public ICollection<SaftCustomer> Customers { get; } = new List<SaftCustomer>();
    /// <summary>Gets the suppliers imported from this exact SAF-T (PT) source.</summary>
    public ICollection<SaftSupplier> Suppliers { get; } = new List<SaftSupplier>();
}
