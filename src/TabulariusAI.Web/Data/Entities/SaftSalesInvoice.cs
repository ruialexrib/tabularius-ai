namespace TabulariusAI.Web.Data.Entities;

public sealed class SaftSalesInvoice
{
    public int Id { get; set; }
    public int SaftImportId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string InvoiceStatus { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public string InvoiceType { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public DateTime? SystemEntryDate { get; set; }
    public decimal TaxPayable { get; set; }
    public decimal NetTotal { get; set; }
    public decimal GrossTotal { get; set; }
    public SaftImport SaftImport { get; set; } = null!;
    public ICollection<SaftSalesInvoiceLine> Lines { get; } = new List<SaftSalesInvoiceLine>();
}

public sealed class SaftSalesInvoiceLine
{
    public int Id { get; set; }
    public int SaftSalesInvoiceId { get; set; }
    public string LineNumber { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public DateOnly? TaxPointDate { get; set; }
    public string? TaxType { get; set; }
    public string? TaxCode { get; set; }
    public decimal? TaxPercentage { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public SaftSalesInvoice SaftSalesInvoice { get; set; } = null!;
    public decimal LineAmount => DebitAmount != 0m ? -DebitAmount : CreditAmount;
}
