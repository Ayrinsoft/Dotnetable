using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Operational money ledger: every inflow, outflow, and analytical component (shipping, markup, cost, profit, …).
/// Transaction types are open-ended strings so new kinds can be added without schema changes.
/// Edits create a new version and keep the previous row (<see cref="SupersedesEntryID"/>).
/// </summary>
public partial class FinancialLedgerEntry
{
    public long FinancialLedgerEntryID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Open-ended type code (e.g. CustomerPayment, OrderMarkup, VendorSettlement).</summary>
    public string TransactionType { get; set; } = null!;

    /// <summary>1 = In (receive), 2 = Out (pay), 3 = Component / memo (breakdown, not cash movement).</summary>
    public byte Flow { get; set; }

    /// <summary>Absolute amount in order/site currency (always ≥ 0). Direction is <see cref="Flow"/>.</summary>
    public decimal Amount { get; set; }

    public decimal AmountUsd { get; set; }

    public string CurrencyCode { get; set; } = null!;

    /// <summary>Local calendar date of the economic event (for date-range reports).</summary>
    public DateOnly OccurredDate { get; set; }

    /// <summary>Local time of the economic event (separate from date for easier filtering).</summary>
    public TimeOnly OccurredTime { get; set; }

    /// <summary>UTC instant for ordering / audit.</summary>
    public DateTime OccurredAtUtc { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>Whether this amount participates in tax filings for the website.</summary>
    public bool ReportToTax { get; set; }

    /// <summary>When true, linked vendors may see this row (settlement/cost only — not markup/profit).</summary>
    public bool VendorVisible { get; set; }

    public int? VendorID { get; set; }

    public int? OrderID { get; set; }

    public int? OrderItemID { get; set; }

    public int? PaymentID { get; set; }

    public int? SettlementID { get; set; }

    public int? WebsiteClientID { get; set; }

    /// <summary>Groups rows posted together for one business event (e.g. one payment breakdown).</summary>
    public Guid EventGroupId { get; set; }

    public int Version { get; set; } = 1;

    /// <summary>Previous version when this row supersedes an edit.</summary>
    public long? SupersedesEntryID { get; set; }

    /// <summary>Only current versions appear in default reports.</summary>
    public bool IsCurrent { get; set; } = true;

    public string? ChangeNote { get; set; }

    public int? CreatedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>Optional JSON bag for extra fields without schema churn.</summary>
    public string? MetaJson { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual Order? Order { get; set; }

    public virtual OrderItem? OrderItem { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual Settlement? Settlement { get; set; }

    public virtual FinancialLedgerEntry? SupersedesEntry { get; set; }

    public virtual Vendor? Vendor { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient? WebsiteClient { get; set; }

    public virtual Currency CurrencyCodeNavigation { get; set; } = null!;
}
