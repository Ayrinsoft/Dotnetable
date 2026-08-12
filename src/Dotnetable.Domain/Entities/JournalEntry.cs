using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class JournalEntry
{
    public int JournalEntryID { get; set; }

    public int WebsiteID { get; set; }

    public string EntryNumber { get; set; } = null!;

    public DateOnly EntryDate { get; set; }

    public string? Description { get; set; }

    /// <summary>Open source category: Manual, FinancialLedger, Payroll, Stock, …</summary>
    public string? SourceType { get; set; }

    /// <summary>Legacy numeric source id when applicable.</summary>
    public int? SourceId { get; set; }

    /// <summary>Idempotency key e.g. L1:{EventGroupId}. Unique per website when set.</summary>
    public string? SourceKey { get; set; }

    public string CurrencyCode { get; set; } = "USD";

    public bool ReportToTax { get; set; } = true;

    public int? FiscalPeriodID { get; set; }

    public bool IsPosted { get; set; }

    public DateTime? PostedAt { get; set; }

    public int? PostedByMemberID { get; set; }

    public bool IsReversed { get; set; }

    public int? ReversesJournalEntryID { get; set; }

    public int? CreatedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual Currency CurrencyCodeNavigation { get; set; } = null!;

    public virtual FiscalPeriod? FiscalPeriod { get; set; }

    public virtual ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();

    public virtual Member? PostedByMember { get; set; }

    public virtual JournalEntry? ReversesJournalEntry { get; set; }

    public virtual Website Website { get; set; } = null!;
}
