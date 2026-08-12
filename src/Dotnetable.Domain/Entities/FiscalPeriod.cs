using System;

namespace Dotnetable.Domain.Entities;

/// <summary>Accounting period per website; closed periods reject new posted journals.</summary>
public partial class FiscalPeriod
{
    public int FiscalPeriodID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly PeriodFrom { get; set; }

    public DateOnly PeriodTo { get; set; }

    public bool IsClosed { get; set; }

    public DateTime? ClosedAt { get; set; }

    public int? ClosedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Member? ClosedByMember { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
}
