using System.ComponentModel.DataAnnotations.Schema;

namespace TabulariusAI.Web.Data.Entities;

[Table("SaftPayments")]
public sealed class SaftPayment
{
    public int Id { get; set; }
    public int SaftImportId { get; set; }
    public string PaymentRefNo { get; set; } = string.Empty;
    public DateOnly TransactionDate { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SourceId { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public DateTime? SystemEntryDate { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal TaxPayable { get; set; }
    public decimal NetTotal { get; set; }
    public decimal GrossTotal { get; set; }
    public SaftImport SaftImport { get; set; } = null!;
    public ICollection<SaftPaymentLine> Lines { get; } = new List<SaftPaymentLine>();
}

[Table("SaftPaymentLines")]
public sealed class SaftPaymentLine
{
    public int Id { get; set; }
    public int SaftPaymentId { get; set; }
    public string LineNumber { get; set; } = string.Empty;
    public string? OriginatingOn { get; set; }
    public DateOnly? InvoiceDate { get; set; }
    public string? Description { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public SaftPayment SaftPayment { get; set; } = null!;
}
