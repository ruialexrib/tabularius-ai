namespace TabulariusAI.Web.Data.Entities;

public sealed class SaftWorkingDocument
{
    public int Id { get; set; }
    public int SaftImportId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string DocumentStatus { get; set; } = string.Empty;
    public DateOnly WorkDate { get; set; }
    public string WorkType { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public DateTime? SystemEntryDate { get; set; }
    public decimal TaxPayable { get; set; }
    public decimal NetTotal { get; set; }
    public decimal GrossTotal { get; set; }
    public SaftImport SaftImport { get; set; } = null!;
    public ICollection<SaftWorkingDocumentLine> Lines { get; } = new List<SaftWorkingDocumentLine>();
}

public sealed class SaftWorkingDocumentLine
{
    public int Id { get; set; }
    public int SaftWorkingDocumentId { get; set; }
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
    public SaftWorkingDocument SaftWorkingDocument { get; set; } = null!;
}
