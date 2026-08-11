using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ClientWalletWithdrawal
{
    public int ClientWalletWithdrawalID { get; set; }

    public int WebsiteID { get; set; }

    public int WebsiteClientID { get; set; }

    public int ClientWalletID { get; set; }

    public int ClientBankAccountID { get; set; }

    /// <summary>Currency of the source wallet (denormalized from <see cref="ClientWallet"/>).</summary>
    public string CurrencyCode { get; set; } = null!;

    /// <summary>Withdrawal amount in <see cref="CurrencyCode"/>.</summary>
    public decimal Amount { get; set; }

    /// <summary>Optional USD mirror for reporting only.</summary>
    public decimal AmountUsd { get; set; }

    public byte Status { get; set; }

    public int? ReviewedByMemberID { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? RejectReason { get; set; }

    public string? PaymentRefNumber { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime RequestedAt { get; set; }

    public virtual ClientBankAccount ClientBankAccount { get; set; } = null!;

    public virtual ClientWallet ClientWallet { get; set; } = null!;

    public virtual Member? ReviewedByMember { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;
}
