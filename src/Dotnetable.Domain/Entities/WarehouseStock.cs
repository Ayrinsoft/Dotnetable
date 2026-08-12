namespace Dotnetable.Domain.Entities;

public partial class WarehouseStock
{
    public int WarehouseStockID { get; set; }
    public int WarehouseID { get; set; }
    public int ProductVariantID { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
