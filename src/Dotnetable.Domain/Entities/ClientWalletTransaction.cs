using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ClientWalletTransaction
{
    public int ClientWalletTransactionID { get; set; }

    public int WebsiteID { get; set; }

    public int ClientWalletID { get; set; }

    public byte Type { get; set; }

    /// <summary>Signed amount in site currency.</summary>
    public decimal Amount { get; set; }

    public decimal AmountUsd { get; set; }

    public decimal BalanceAfter { get; set; }

    public decimal BalanceAfterUsd { get; set; }

    public byte? SourceType { get; set; }

    public int? SourceId { get; set; }

    public string? Note { get; set; }

    public int? CreatedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ClientWallet ClientWallet { get; set; } = null!;

    public virtual Member? CreatedByMember { get; set; }

    public virtual ICollection<PaymentRefund> PaymentRefunds { get; set; } = new List<PaymentRefund>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Website Website { get; set; } = null!;
}
