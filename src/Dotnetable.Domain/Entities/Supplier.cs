using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Supplier
{
    public int SupplierID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual Website Website { get; set; } = null!;
}
