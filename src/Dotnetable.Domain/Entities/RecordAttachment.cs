using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Scanned paper / supporting file linked to any business record
/// (order, payment, stock doc, payroll, contract, journal, settlement, …).
/// </summary>
public partial class RecordAttachment
{
    public long RecordAttachmentID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Open type key, e.g. Order, Payment, StockDocument, EmployeeContract, PayrollRun, JournalEntry, Settlement.</summary>
    public string EntityType { get; set; } = null!;

    /// <summary>Primary key of the owning entity (int cast to long when needed).</summary>
    public long EntityID { get; set; }

    public int FileRecordID { get; set; }

    public string? Title { get; set; }

    public string? Note { get; set; }

    public int? CreatedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual FileRecord FileRecord { get; set; } = null!;

    public virtual Member? CreatedByMember { get; set; }
}
