using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Vendor
{
    public int VendorID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int? LogoFileID { get; set; }

    public decimal Rating { get; set; }

    public bool IsActive { get; set; }

    /// <summary>
    /// 0 = Immediate: every purchase from this vendor is settled instantly like a normal cash purchase. 1 = Credit: purchases accrue as credit and are batched into a periodic Settlements record due on CreditDays
    /// </summary>
    public byte SettlementMode { get; set; }

    /// <summary>
    /// number of days after the settlement period ends before payment is due; only meaningful when SettlementMode = Credit
    /// </summary>
    public int? CreditDays { get; set; }

    /// <summary>
    /// Maximum outstanding credit in site currency; only meaningful when SettlementMode = Credit.
    /// </summary>
    public decimal? CreditLimit { get; set; }

    /// <summary>
    /// maximum outstanding credit balance allowed for this vendor, in USD; only meaningful when SettlementMode = Credit
    /// </summary>
    public decimal? CreditLimitUsd { get; set; }

    /// <summary>
    /// 0 = Display-only title, 1 = Member login (manage own catalog), 2 = Linked website (inter-site virtual credit)
    /// </summary>
    public byte VendorType { get; set; }

    public int? MemberID { get; set; }

    public int? LinkedWebsiteID { get; set; }

    /// <summary>Available inter-site / credit balance in site currency.</summary>
    public decimal AvailableCredit { get; set; }

    public decimal AvailableCreditUsd { get; set; }

    public virtual Website? LinkedWebsite { get; set; }

    public virtual FileRecord? LogoFile { get; set; }

    public virtual Member? Member { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductAnswer> ProductAnswers { get; set; } = new List<ProductAnswer>();

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual ICollection<VendorCreditTransaction> VendorCreditTransactions { get; set; } = new List<VendorCreditTransaction>();

    public virtual ICollection<VendorProduct> VendorProducts { get; set; } = new List<VendorProduct>();

    public virtual ICollection<VendorTranslation> VendorTranslations { get; set; } = new List<VendorTranslation>();

    public virtual Website Website { get; set; } = null!;
}
