namespace TabulariusAI.Web.Models;

public sealed class SaftPaymentViewModel
{
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
    public IList<SaftPaymentLineViewModel> Lines { get; } = new List<SaftPaymentLineViewModel>();
}

public sealed class SaftPaymentLineViewModel
{
    public string LineNumber { get; set; } = string.Empty;
    public string? OriginatingOn { get; set; }
    public DateOnly? InvoiceDate { get; set; }
    public string? Description { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
}
