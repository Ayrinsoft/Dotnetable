using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class PaymentRefund
{
    public int PaymentRefundID { get; set; }

    public int PaymentID { get; set; }

    public decimal Amount { get; set; }

    public string? Reason { get; set; }

    public byte Status { get; set; }

    public int? BankAccountID { get; set; }

    public int? ClientWalletTransactionID { get; set; }

    public DateTime? RefundedAt { get; set; }

    public int? CreatedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual BankAccount? BankAccount { get; set; }

    public virtual ClientWalletTransaction? ClientWalletTransaction { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual Payment Payment { get; set; } = null!;
}
