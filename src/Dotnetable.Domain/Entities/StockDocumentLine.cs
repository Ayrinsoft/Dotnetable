namespace Dotnetable.Domain.Entities;

public partial class StockDocumentLine
{
    public int StockDocumentLineID { get; set; }
    public int StockDocumentID { get; set; }
    public int ProductVariantID { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal UnitCostUsd { get; set; }
    public string? Note { get; set; }

    public virtual StockDocument StockDocument { get; set; } = null!;
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
