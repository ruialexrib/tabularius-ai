namespace TabulariusAI.Web.Models;

/// <summary>
/// Represents the SAF-T (PT) header and structural summary displayed after a successful file analysis.
/// </summary>
public sealed class SaftHeaderViewModel
{
    /// <summary>Gets or sets the SAF-T (PT) version declared in the document header.</summary>
    public string SaftVersion { get; set; } = string.Empty;
    /// <summary>Gets or sets the tax registration number of the company represented by the SAF-T file.</summary>
    public string TaxRegistrationNumber { get; set; } = string.Empty;
    /// <summary>Gets or sets the company name declared in the SAF-T header.</summary>
    public string CompanyName { get; set; } = string.Empty;
    /// <summary>Gets or sets the fiscal year declared in the SAF-T header.</summary>
    public string FiscalYear { get; set; } = string.Empty;
    /// <summary>Gets or sets the start date of the accounting period represented by the file.</summary>
    public string StartDate { get; set; } = string.Empty;
    /// <summary>Gets or sets the end date of the accounting period represented by the file.</summary>
    public string EndDate { get; set; } = string.Empty;
    /// <summary>Gets or sets the product identifier of the software that generated the SAF-T file.</summary>
    public string ProductId { get; set; } = string.Empty;
    /// <summary>Gets or sets the product version of the software that generated the SAF-T file.</summary>
    public string ProductVersion { get; set; } = string.Empty;
    /// <summary>Gets or sets the number of general ledger accounts declared in MasterFiles.</summary>
    public int AccountCount { get; set; }
    /// <summary>Gets or sets the number of customers declared in MasterFiles.</summary>
    public int CustomerCount { get; set; }
    /// <summary>Gets or sets the number of suppliers declared in MasterFiles.</summary>
    public int SupplierCount { get; set; }
    /// <summary>Gets or sets the number of products or services declared in MasterFiles.</summary>
    public int ProductCount { get; set; }
    /// <summary>Gets or sets the number of accounting transactions declared in GeneralLedgerEntries.</summary>
    public int TransactionCount { get; set; }
    /// <summary>Gets or sets the number of sales invoices declared in SourceDocuments.</summary>
    public int SalesInvoiceCount { get; set; }
    /// <summary>Gets or sets the number of movement of goods documents declared in SourceDocuments.</summary>
    public int MovementOfGoodsCount { get; set; }
    /// <summary>Gets or sets the number of working documents declared in SourceDocuments.</summary>
    public int WorkingDocumentCount { get; set; }
    /// <summary>Gets or sets the number of payment documents declared in SourceDocuments.</summary>
    public int PaymentCount { get; set; }
    /// <summary>Gets the general ledger accounts parsed from the SAF-T (PT) master files section.</summary>
    public IList<SaftAccountViewModel> Accounts { get; } = new List<SaftAccountViewModel>();
}
