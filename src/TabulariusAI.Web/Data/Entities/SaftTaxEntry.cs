namespace TabulariusAI.Web.Data.Entities;

/// <summary>Represents one tax definition imported from SAF-T (PT) MasterFiles/TaxTable.</summary>
public sealed class SaftTaxEntry
{
    public int Id { get; set; }
    public int SaftImportId { get; set; }
    public string TaxType { get; set; } = string.Empty;
    public string TaxCountryRegion { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly? TaxExpirationDate { get; set; }
    public decimal? TaxPercentage { get; set; }
    public decimal? TaxAmount { get; set; }
    public SaftImport SaftImport { get; set; } = null!;
}
