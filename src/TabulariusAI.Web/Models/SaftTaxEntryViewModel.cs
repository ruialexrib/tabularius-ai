namespace TabulariusAI.Web.Models;

/// <summary>Represents one SAF-T (PT) MasterFiles/TaxTable/TaxTableEntry record.</summary>
public sealed class SaftTaxEntryViewModel
{
    public string TaxType { get; set; } = string.Empty;
    public string TaxCountryRegion { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly? TaxExpirationDate { get; set; }
    public decimal? TaxPercentage { get; set; }
    public decimal? TaxAmount { get; set; }
}
