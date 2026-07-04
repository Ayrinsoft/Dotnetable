using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Values for <see cref="Payment"/>.Method (TINYINT). No gateway integration in this build —
/// only wallet debit and manual/offline bank transfer are supported.</summary>
public enum PaymentMethod : byte
{
    Wallet = 1,
    BankTransfer = 2,
}

/// <summary>Values for <see cref="Payment"/>.Status (TINYINT).</summary>
public enum PaymentStatus : byte
{
    Pending = 1,
    Paid = 2,
    Rejected = 3,
    Refunded = 4,
}

/// <summary>Values for <see cref="PaymentRefund"/>.Status (TINYINT).</summary>
public enum PaymentRefundStatus : byte
{
    Pending = 1,
    Completed = 2,
}

/// <summary>
/// Records payments against an order and settles them either instantly (wallet debit) or after manual
/// admin verification (offline bank transfer + uploaded receipt). No payment-gateway abstraction exists
/// by design — this build only supports wallet balance and manual bank transfer.
/// </summary>
public interface IPaymentService
{
    /// <summary>Debits the order's grand total from the customer's wallet and marks the order Paid immediately.</summary>
    Task<(bool Success, string? Error, Payment? Payment)> PayWithWalletAsync(int websiteId, int clientId, int orderId, CancellationToken ct = default);

    /// <summary>Records a Pending payment referencing an uploaded bank-transfer receipt; the order stays
    /// unpaid until an admin verifies it.</summary>
    Task<(bool Success, string? Error, Payment? Payment)> SubmitBankReceiptAsync(
        int websiteId, int clientId, int orderId, int bankAccountId, int receiptFileId, CancellationToken ct = default);

    /// <summary>Admin verification of a pending manual payment: approve marks it Paid and transitions the
    /// order to Paid; reject marks it Rejected and leaves the order unpaid (customer may resubmit).</summary>
    Task<bool> VerifyAsync(int paymentId, int memberId, bool approve, string? note, CancellationToken ct = default);

    Task<Payment?> GetByIdAsync(int paymentId, CancellationToken ct = default);

    Task<Payment?> GetLatestForOrderAsync(int orderId, CancellationToken ct = default);

    Task<Application.DTOs.PagedResult<Payment>> GetPendingAsync(int? websiteId, Application.DTOs.GridQuery query, CancellationToken ct = default);

    /// <summary>Refunds a payment, either crediting the customer's wallet (instant) or recording a
    /// Pending bank refund for manual off-system transfer (admin marks it Completed once done).</summary>
    Task<(bool Success, string? Error, PaymentRefund? Refund)> RefundAsync(
        int paymentId, decimal amountUsd, string? reason, bool toWallet, int? bankAccountId, int memberId, CancellationToken ct = default);

    Task<bool> CompleteBankRefundAsync(int paymentRefundId, int memberId, CancellationToken ct = default);

    /// <summary>Bank refunds awaiting the admin to perform the manual outgoing transfer and mark it done.</summary>
    Task<Application.DTOs.PagedResult<PaymentRefund>> GetPendingRefundsAsync(int? websiteId, Application.DTOs.GridQuery query, CancellationToken ct = default);
}
