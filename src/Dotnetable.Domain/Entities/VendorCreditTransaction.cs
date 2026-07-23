namespace Dotnetable.Domain.Entities;

/// <summary>
/// Ledger of virtual credit between a host website and a site-linked (or credit-mode) vendor.
/// Positive amounts grant/restore credit; negative amounts consume it on sale.
/// </summary>
public partial class VendorCreditTransaction
{
    public int VendorCreditTransactionID { get; set; }

    public int VendorID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Signed amount in USD (+ grant/refund, − sale).</summary>
    public decimal AmountUsd { get; set; }

    public decimal BalanceAfterUsd { get; set; }

    /// <summary>1 = Grant, 2 = Sale, 3 = Adjustment, 4 = Refund.</summary>
    public byte SourceType { get; set; }

    public int? SourceOrderItemID { get; set; }

    public int? MirrorOrderID { get; set; }

    public string? Note { get; set; }

    public int? CreatedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual OrderItem? SourceOrderItem { get; set; }

    public virtual Order? MirrorOrder { get; set; }

    public virtual Vendor Vendor { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;
}
