using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Payment
{
    public int PaymentID { get; set; }

    public int WebsiteID { get; set; }

    public int? OrderID { get; set; }

    public int WebsiteClientID { get; set; }

    public byte Method { get; set; }

    public int? PaymentGatewayID { get; set; }

    public int? BankAccountID { get; set; }

    public int? ClientWalletTransactionID { get; set; }

    public decimal Amount { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public decimal ExchangeRateToUsd { get; set; }

    public decimal AmountUsd { get; set; }

    public byte Status { get; set; }

    public string? GatewayRefNumber { get; set; }

    /// <summary>Pre-verification handle the PSP issued when the payment started (Zarinpal authority, Stripe PaymentIntent id, ...). Used to verify the callback.</summary>
    public string? GatewayAuthority { get; set; }

    public string? TrackingCode { get; set; }

    public int? ReceiptFileID { get; set; }

    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Admin member who created this payment row (e.g. offline/COD receipt).
    /// Null means the customer initiated it (wallet pay or bank-receipt upload).
    /// </summary>
    public int? CreatedByMemberID { get; set; }

    /// <summary>Admin who approved a pending bank-transfer receipt (or who recorded an already-paid offline payment).</summary>
    public int? VerifiedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual BankAccount? BankAccount { get; set; }

    public virtual ClientWalletTransaction? ClientWalletTransaction { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual Currency CurrencyCodeNavigation { get; set; } = null!;

    public virtual Order? Order { get; set; }

    public virtual PaymentGateway? PaymentGateway { get; set; }

    public virtual ICollection<PaymentRefund> PaymentRefunds { get; set; } = new List<PaymentRefund>();

    public virtual FileRecord? ReceiptFile { get; set; }

    public virtual ICollection<SettlementItem> SettlementItems { get; set; } = new List<SettlementItem>();

    public virtual Member? VerifiedByMember { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;
}
