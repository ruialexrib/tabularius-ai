namespace TabulariusAI.Web.Models;

/// <summary>Represents a SAF-T (PT) movement of goods document imported from SourceDocuments.</summary>
public sealed class SaftStockMovementViewModel
{
    /// <summary>Gets or sets the movement document number.</summary>
    public string DocumentNumber { get; set; } = string.Empty;
    /// <summary>Gets or sets the movement document status.</summary>
    public string DocumentStatus { get; set; } = string.Empty;
    /// <summary>Gets or sets the movement document date.</summary>
    public DateOnly MovementDate { get; set; }
    /// <summary>Gets or sets the movement document type.</summary>
    public string MovementType { get; set; } = string.Empty;
    /// <summary>Gets or sets the source user identifier.</summary>
    public string SourceId { get; set; } = string.Empty;
    /// <summary>Gets or sets the customer identifier, when present.</summary>
    public string? CustomerId { get; set; }
    /// <summary>Gets or sets the supplier identifier, when present.</summary>
    public string? SupplierId { get; set; }
    /// <summary>Gets or sets the system entry date.</summary>
    public DateTime? SystemEntryDate { get; set; }
    /// <summary>Gets the document lines.</summary>
    public IList<SaftStockMovementLineViewModel> Lines { get; } = new List<SaftStockMovementLineViewModel>();
}

/// <summary>Represents a line of a SAF-T (PT) movement of goods document.</summary>
public sealed class SaftStockMovementLineViewModel
{
    /// <summary>Gets or sets the source line number.</summary>
    public string LineNumber { get; set; } = string.Empty;
    /// <summary>Gets or sets the product code.</summary>
    public string ProductCode { get; set; } = string.Empty;
    /// <summary>Gets or sets the product description.</summary>
    public string ProductDescription { get; set; } = string.Empty;
    /// <summary>Gets or sets the moved quantity.</summary>
    public decimal Quantity { get; set; }
    /// <summary>Gets or sets the unit of measure.</summary>
    public string UnitOfMeasure { get; set; } = string.Empty;
    /// <summary>Gets or sets the unit price when supplied by the source.</summary>
    public decimal? UnitPrice { get; set; }
}
