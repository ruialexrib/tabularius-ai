namespace TabulariusAI.Web.Data.Entities;

/// <summary>
/// Represents a company or organisation whose accounting information is analysed in Tabularius AI.
/// </summary>
public sealed class AccountingEntity
{
    /// <summary>Gets or sets the entity identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the entity name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the Portuguese tax registration number.</summary>
    public string TaxRegistrationNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC date and time when the entity was created.</summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Gets the analysis dossiers associated with the entity.</summary>
    public ICollection<AnalysisDossier> Dossiers { get; } = new List<AnalysisDossier>();
}
