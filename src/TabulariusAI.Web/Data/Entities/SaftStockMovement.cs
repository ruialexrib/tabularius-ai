namespace TabulariusAI.Web.Data.Entities;

/// <summary>Represents a SAF-T (PT) movement of goods document.</summary>
public sealed class SaftStockMovement
{
    public int Id { get; set; }
    public int SaftImportId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string DocumentStatus { get; set; } = string.Empty;
    public DateOnly MovementDate { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public string? SupplierId { get; set; }
    public DateTime? SystemEntryDate { get; set; }
    public SaftImport SaftImport { get; set; } = null!;
    public ICollection<SaftStockMovementLine> Lines { get; } = new List<SaftStockMovementLine>();
}

/// <summary>Represents a line of a SAF-T (PT) movement of goods document.</summary>
public sealed class SaftStockMovementLine
{
    public int Id { get; set; }
    public int SaftStockMovementId { get; set; }
    public string LineNumber { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal? UnitPrice { get; set; }
    public SaftStockMovement SaftStockMovement { get; set; } = null!;
}
