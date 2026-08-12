using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Accounting period per website. Closed periods reject new journals and only appear in historical reports.
/// Closing creates an opening-balance journal for the next period (balance sheet carry-forward).
/// </summary>
public partial class FiscalPeriod
{
    public int FiscalPeriodID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly PeriodFrom { get; set; }

    public DateOnly PeriodTo { get; set; }

    /// <summary>Soft due date for the owner to close this period (PeriodTo + site FiscalCloseDueDays).</summary>
    public DateOnly? CloseDueDate { get; set; }

    public bool IsClosed { get; set; }

    public DateTime? ClosedAt { get; set; }

    public int? ClosedByMemberID { get; set; }

    /// <summary>Posted journal that brought opening balances into this period (null for first period).</summary>
    public int? OpeningJournalEntryID { get; set; }

    /// <summary>Posted closing / P&amp;L roll-forward journal created when this period was closed.</summary>
    public int? ClosingJournalEntryID { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Member? ClosedByMember { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual JournalEntry? OpeningJournalEntry { get; set; }

    public virtual JournalEntry? ClosingJournalEntry { get; set; }

    public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
}
