using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Settlement
{
    public int SettlementID { get; set; }

    public int WebsiteID { get; set; }

    public byte TargetType { get; set; }

    public int? VendorID { get; set; }

    public int? TargetWebsiteID { get; set; }

    public int? SupplierID { get; set; }

    public DateOnly PeriodFrom { get; set; }

    public DateOnly PeriodTo { get; set; }

    public decimal TotalAmount { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public byte Status { get; set; } = (byte)1;

    public int? BankAccountID { get; set; }

    public string? PaymentRefNumber { get; set; }

    public string? Note { get; set; }

    public int? CreatedByMemberID { get; set; }

    public int? ApprovedByMemberID { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Member? ApprovedByMember { get; set; }

    public virtual BankAccount? BankAccount { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual Currency CurrencyCodeNavigation { get; set; } = null!;

    public virtual ICollection<SettlementItem> SettlementItems { get; set; } = new List<SettlementItem>();

    public virtual Supplier? Supplier { get; set; }

    public virtual Website? TargetWebsite { get; set; }

    public virtual Vendor? Vendor { get; set; }

    public virtual Website Website { get; set; } = null!;
}
