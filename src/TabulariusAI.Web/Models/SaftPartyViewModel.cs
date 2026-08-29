namespace TabulariusAI.Web.Models;

/// <summary>
/// Represents customer or supplier master data parsed from a SAF-T (PT) import.
/// </summary>
public sealed class SaftPartyViewModel
{
    /// <summary>Gets or sets the source customer or supplier identifier.</summary>
    public string PartyId { get; set; } = string.Empty;
    /// <summary>Gets or sets the associated source ledger account identifier.</summary>
    public string AccountId { get; set; } = string.Empty;
    /// <summary>Gets or sets the tax identifier declared for the party.</summary>
    public string TaxId { get; set; } = string.Empty;
    /// <summary>Gets or sets the company or person name declared in the source record.</summary>
    public string CompanyName { get; set; } = string.Empty;
}
