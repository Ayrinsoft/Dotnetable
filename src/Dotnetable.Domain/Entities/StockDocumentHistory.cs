using System;

namespace Dotnetable.Domain.Entities;

public partial class StockDocumentHistory
{
    public int StockDocumentHistoryID { get; set; }
    public int StockDocumentID { get; set; }
    public byte FromStatus { get; set; }
    public byte ToStatus { get; set; }
    public string? Note { get; set; }
    public int? CreatedByMemberID { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual StockDocument StockDocument { get; set; } = null!;
    public virtual Member? CreatedByMember { get; set; }
}
