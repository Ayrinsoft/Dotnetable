using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>Warehouse document header. Only <c>Posted</c> docs change stock balances.</summary>
public partial class StockDocument
{
    public int StockDocumentID { get; set; }
    public int WebsiteID { get; set; }
    public string DocumentNumber { get; set; } = null!;
    /// <summary><see cref="Enums.StockDocumentType"/>.</summary>
    public byte DocumentType { get; set; }
    /// <summary><see cref="Enums.StockDocumentStatus"/>.</summary>
    public byte Status { get; set; }
    public int? FromWarehouseID { get; set; }
    public int? ToWarehouseID { get; set; }
    public int? SupplierID { get; set; }
    public int? OrderID { get; set; }
    public string? Note { get; set; }
    public int? RequestedByMemberID { get; set; }
    public int? ApprovedByMemberID { get; set; }
    public int? PostedByMemberID { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
    public virtual Warehouse? FromWarehouse { get; set; }
    public virtual Warehouse? ToWarehouse { get; set; }
    public virtual Supplier? Supplier { get; set; }
    public virtual Order? Order { get; set; }
    public virtual Member? RequestedByMember { get; set; }
    public virtual Member? ApprovedByMember { get; set; }
    public virtual Member? PostedByMember { get; set; }
    public virtual ICollection<StockDocumentLine> StockDocumentLines { get; set; } = new List<StockDocumentLine>();
    public virtual ICollection<StockDocumentHistory> StockDocumentHistories { get; set; } = new List<StockDocumentHistory>();
}
