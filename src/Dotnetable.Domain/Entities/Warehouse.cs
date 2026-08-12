using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Warehouse
{
    public int WarehouseID { get; set; }
    public int WebsiteID { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
    public virtual ICollection<WarehouseStock> WarehouseStocks { get; set; } = new List<WarehouseStock>();
    public virtual ICollection<StockDocument> StockDocumentFromWarehouses { get; set; } = new List<StockDocument>();
    public virtual ICollection<StockDocument> StockDocumentToWarehouses { get; set; } = new List<StockDocument>();
}
