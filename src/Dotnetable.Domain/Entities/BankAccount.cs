using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class BankAccount
{
    public int BankAccountID { get; set; }

    public int BankID { get; set; }

    public int WebsiteID { get; set; }

    public string Title { get; set; } = null!;

    public string? OwnerName { get; set; }

    public string? AccountNumber { get; set; }

    public string? IBAN { get; set; }

    public string? CardNumber { get; set; }

    public bool IsForOfflinePayment { get; set; }

    public bool IsActive { get; set; }

    public int CreatedByMemberId { get; set; }

    public virtual Bank Bank { get; set; } = null!;

    public virtual Member CreatedByMember { get; set; } = null!;

    public virtual ICollection<PaymentRefund> PaymentRefunds { get; set; } = new List<PaymentRefund>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual Website Website { get; set; } = null!;
}
