using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Values for <see cref="Payment"/>.Method (TINYINT). No gateway integration in this build —
/// wallet debit, offline bank transfer, and admin-recorded cash/COD/manual receipts are supported.</summary>
public enum PaymentMethod : byte
{
    Wallet = 1,
    BankTransfer = 2,
    /// <summary>Admin-recorded offline collection (cash, POS, wire outside the receipt queue, etc.).</summary>
    Manual = 3,
    /// <summary>Cash (or card) collected on delivery — recorded by admin after collection.</summary>
    CashOnDelivery = 4,
}

/// <summary>Values for <see cref="Payment"/>.Status (TINYINT).</summary>
public enum PaymentStatus : byte
{
    Pending = 1,
    Paid = 2,
    Rejected = 3,
    Refunded = 4,
}

/// <summary>Payment statuses that still need admin action on the offline bank-transfer queue
/// (excludes Paid / Rejected / Refunded which are terminal for verification).</summary>
public static class PaymentStatusQueues
{
    public static readonly PaymentStatus[] Actionable =
    [
        PaymentStatus.Pending,
    ];
}

/// <summary>Values for <see cref="PaymentRefund"/>.Status (TINYINT).</summary>
public enum PaymentRefundStatus : byte
{
    Pending = 1,
    Completed = 2,
}

/// <summary>Bank-refund statuses that still need admin action (excludes Completed).</summary>
public static class PaymentRefundStatusQueues
{
    public static readonly PaymentRefundStatus[] Actionable =
    [
        PaymentRefundStatus.Pending,
    ];
}

/// <summary>
/// Records payments against an order and settles them either instantly (wallet debit), after manual
/// admin verification (offline bank transfer + uploaded receipt), or via admin-recorded cash/COD/manual
/// receipt. No payment-gateway abstraction exists by design.
/// </summary>
public interface IPaymentService
{
    /// <summary>Debits the order's grand total from the customer's wallet and marks the order Paid immediately.</summary>
    Task<(bool Success, string? Error, Payment? Payment)> PayWithWalletAsync(int websiteId, int clientId, int orderId, CancellationToken ct = default);

    /// <summary>Records a Pending payment referencing an uploaded bank-transfer receipt; the order stays
    /// unpaid until an admin verifies it.</summary>
    Task<(bool Success, string? Error, Payment? Payment)> SubmitBankReceiptAsync(
        int websiteId, int clientId, int orderId, int bankAccountId, int receiptFileId, CancellationToken ct = default);

    /// <summary>
    /// Admin records that money was received from the customer outside the self-serve flows
    /// (cash, COD collection, POS, card-to-card / bank transfer with optional receipt, etc.).
    /// When <paramref name="markAsPaid"/> is true (default), creates a Paid payment and, when the order
    /// is still <see cref="OrderStatus.PendingPayment"/>, transitions it to Paid.
    /// When false and method is bank transfer, creates a Pending receipt for the verification queue.
    /// </summary>
    /// <param name="method">
    /// <see cref="PaymentMethod.Manual"/>, <see cref="PaymentMethod.CashOnDelivery"/>,
    /// or <see cref="PaymentMethod.BankTransfer"/>.
    /// </param>
    /// <param name="amountLocal">Optional amount in the order currency; defaults to the order grand total.</param>
    /// <param name="reference">Optional tracking / receipt reference stored on the payment.</param>
    /// <param name="note">Optional free-text note (stored in <see cref="Payment.GatewayRefNumber"/> for audit).</param>
    /// <param name="bankAccountId">Destination bank account for card-to-card / bank transfer.</param>
    /// <param name="receiptFileId">Uploaded receipt image/file id.</param>
    /// <param name="markAsPaid">When true, payment is Paid immediately; when false for bank transfer, stays Pending.</param>
    /// <param name="paidAtUtc">When money was received (UTC). When null and paid, uses UtcNow.</param>
    Task<(bool Success, string? Error, Payment? Payment)> RecordReceivedPaymentAsync(
        int orderId, PaymentMethod method, decimal? amountLocal, string? reference, string? note,
        int memberId, int? bankAccountId = null, int? receiptFileId = null, bool markAsPaid = true,
        DateTime? paidAtUtc = null, CancellationToken ct = default);

    /// <summary>
    /// Admin records money received for a customer with no order (wallet top-up). Creates a Paid
    /// payment (CreatedByMemberID set) and credits the matching currency wallet.
    /// </summary>
    Task<(bool Success, string? Error, Payment? Payment)> RecordWalletDepositAsync(
        int websiteId, int clientId, decimal amount, string? currencyCode,
        int? bankAccountId, string? description, string? reference, int? receiptFileId,
        int memberId, DateTime? paidAtUtc = null, CancellationToken ct = default);

    /// <summary>Admin verification of a pending manual payment: approve marks it Paid and transitions the
    /// order to Paid; reject marks it Rejected and leaves the order unpaid (customer may resubmit).</summary>
    Task<bool> VerifyAsync(int paymentId, int memberId, bool approve, string? note, CancellationToken ct = default);

    Task<Payment?> GetByIdAsync(int paymentId, CancellationToken ct = default);

    Task<Payment?> GetLatestForOrderAsync(int orderId, CancellationToken ct = default);

    /// <summary>Pending bank-transfer receipts awaiting verification. Prefer
    /// <see cref="GetManualPagedAsync"/> when a status filter is needed.</summary>
    Task<PagedResult<Payment>> GetPendingAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    /// <summary>Bank-transfer payments for the admin verification queue, optionally filtered by status.</summary>
    Task<PagedResult<Payment>> GetManualPagedAsync(int? websiteId, byte? status, GridQuery query, CancellationToken ct = default);

    /// <summary>Counts of bank-transfer payments per <see cref="Payment"/>.Status for the optional website scope.</summary>
    Task<IReadOnlyDictionary<byte, int>> GetManualStatusCountsAsync(int? websiteId, CancellationToken ct = default);

    /// <summary>
    /// Refunds a paid payment. <paramref name="amount"/> is in the payment's own currency
    /// (site currency for storefront/admin orders — same unit as <see cref="Payment.Amount"/>),
    /// not USD. Dual USD on the payment is reporting-only.
    /// Destination is exactly one of: wallet credit (instant, same currency), bank account
    /// (Pending until marked completed), or cash/manual
    /// (<paramref name="toWallet"/> false and <paramref name="bankAccountId"/> null — completed immediately).
    /// </summary>
    Task<(bool Success, string? Error, PaymentRefund? Refund)> RefundAsync(
        int paymentId, decimal amount, string? reason, bool toWallet, int? bankAccountId, int memberId,
        bool markCompleted = false, CancellationToken ct = default);

    /// <summary>Paid (or partially refunded) payments that still have a refundable remainder.</summary>
    Task<IReadOnlyList<RefundablePaymentDto>> GetRefundablePaymentsAsync(
        int websiteId, int clientId, CancellationToken ct = default);

    Task<bool> CompleteBankRefundAsync(int paymentRefundId, int memberId, CancellationToken ct = default);

    /// <summary>Bank refunds awaiting the admin to perform the manual outgoing transfer and mark it done.
    /// Prefer <see cref="GetBankRefundsPagedAsync"/> when a status filter is needed.</summary>
    Task<PagedResult<PaymentRefund>> GetPendingRefundsAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    /// <summary>Manual bank refunds for the admin queue, optionally filtered by status (only rows with a bank account).</summary>
    Task<PagedResult<PaymentRefund>> GetBankRefundsPagedAsync(int? websiteId, byte? status, GridQuery query, CancellationToken ct = default);

    /// <summary>Counts of bank refunds per <see cref="PaymentRefund"/>.Status for the optional website scope.</summary>
    Task<IReadOnlyDictionary<byte, int>> GetBankRefundStatusCountsAsync(int? websiteId, CancellationToken ct = default);
}
