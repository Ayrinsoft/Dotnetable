using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class SettlementItem
{
    public int SettlementItemID { get; set; }

    public int SettlementID { get; set; }

    public int? OrderItemID { get; set; }

    public int? StockMovementID { get; set; }

    public int? PaymentID { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public virtual OrderItem? OrderItem { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual Settlement Settlement { get; set; } = null!;

    public virtual StockMovement? StockMovement { get; set; }
}
