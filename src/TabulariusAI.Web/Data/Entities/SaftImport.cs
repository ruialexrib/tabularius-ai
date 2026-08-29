namespace TabulariusAI.Web.Data.Entities;

/// <summary>Represents metadata about a SAF-T (PT) file imported into an analysis dossier.</summary>
public sealed class SaftImport
{
    public int Id { get; set; }
    public int DossierId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string? ContentHash { get; set; }
    public string SaftVersion { get; set; } = string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTime ImportedAtUtc { get; set; } = DateTime.UtcNow;
    public AnalysisDossier Dossier { get; set; } = null!;
    public ICollection<SaftAccount> Accounts { get; } = new List<SaftAccount>();
    public ICollection<SaftCustomer> Customers { get; } = new List<SaftCustomer>();
    public ICollection<SaftSupplier> Suppliers { get; } = new List<SaftSupplier>();
    public ICollection<SaftProduct> Products { get; } = new List<SaftProduct>();
    public ICollection<SaftTransaction> Transactions { get; } = new List<SaftTransaction>();
    public ICollection<SaftSalesInvoice> SalesInvoices { get; } = new List<SaftSalesInvoice>();
    public ICollection<SaftStockMovement> StockMovements { get; } = new List<SaftStockMovement>();
}
