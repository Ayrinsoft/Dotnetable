namespace Dotnetable.Domain.Entities;

public partial class StockDocumentLine
{
    public int StockDocumentLineID { get; set; }
    public int StockDocumentID { get; set; }
    public int ProductVariantID { get; set; }
    /// <summary>
    /// Movement quantity for Inbound/Outbound/Transfer/Adjustment/Return.
    /// For Count documents: stored variance (Counted − Book) used when posting.
    /// </summary>
    public int Quantity { get; set; }
    /// <summary>System book qty snapshot (warehouse on-hand) for Count lines.</summary>
    public int BookQuantity { get; set; }
    /// <summary>Physical counted qty for Count lines; null for non-count docs.</summary>
    public int? CountedQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal UnitCostUsd { get; set; }
    public string? Note { get; set; }

    /// <summary>
    /// Item condition: <see cref="Enums.StockItemCondition"/> (New/OpenBox/Display/Used/Defective…).
    /// Column name kept as ReturnCondition for DB compatibility.
    /// </summary>
    public byte ReturnCondition { get; set; }

    /// <summary>
    /// Health grade for non-new stock: <see cref="Enums.StockHealthGrade"/> (0 = n/a for New).
    /// Required when condition is not New / None / Defective (Defective optional).
    /// </summary>
    public byte HealthGrade { get; set; }

    public virtual StockDocument StockDocument { get; set; } = null!;
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
