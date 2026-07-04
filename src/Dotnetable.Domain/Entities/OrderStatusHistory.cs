using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class OrderStatusHistory
{
    public int OrderStatusHistoryID { get; set; }

    public int OrderID { get; set; }

    public byte? FromStatus { get; set; }

    public byte ToStatus { get; set; }

    public string? Note { get; set; }

    public int? CreatedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual Order Order { get; set; } = null!;
}
