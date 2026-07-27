using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly IClientWalletService _wallet;
    private readonly IOrderService _orders;
    private readonly IAdminNotificationService _notifications;

    public PaymentService(
        AppDbContext context, IClientWalletService wallet, IOrderService orders,
        IAdminNotificationService notifications)
    {
        _context = context;
        _wallet = wallet;
        _orders = orders;
        _notifications = notifications;
    }

    public async Task<(bool Success, string? Error, Payment? Payment)> PayWithWalletAsync(int websiteId, int clientId, int orderId, CancellationToken ct = default)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderId && o.WebsiteClientID == clientId, ct);
        if (order is null) return (false, "Order not found.", null);
        if (order.Status != (byte)OrderStatus.PendingPayment) return (false, "Order is not awaiting payment.", null);

        ClientWalletTransaction walletTx;
        try
        {
            walletTx = await _wallet.ApplyAsync(
                websiteId, clientId, (byte)ClientWalletTransactionType.PurchaseUse, -order.GrandTotalUsd,
                (byte)ClientWalletSourceType.Payment, orderId, $"Order {order.OrderNumber}", null, ct);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message, null);
        }

        var payment = new Payment
        {
            WebsiteID = websiteId,
            OrderID = orderId,
            WebsiteClientID = clientId,
            Method = (byte)PaymentMethod.Wallet,
            ClientWalletTransactionID = walletTx.ClientWalletTransactionID,
            Amount = order.GrandTotal,
            CurrencyCode = order.CurrencyCode,
            ExchangeRateToUsd = order.ExchangeRateToUsd,
            AmountUsd = order.GrandTotalUsd,
            Status = (byte)PaymentStatus.Paid,
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(ct);

        await _orders.TransitionStatusAsync(orderId, OrderStatus.Paid, null, "Paid with wallet balance.", ct);

        await _notifications.NotifySiteAdminsAsync(
            websiteId,
            AdminNotificationType.PaymentReceived,
            "Payment received",
            $"Order #{order.OrderNumber} paid with wallet — {order.GrandTotal:0.##} {order.CurrencyCode}.",
            $"/orders/{order.OrderID}",
            payment.PaymentID,
            ct);

        return (true, null, payment);
    }

    public async Task<(bool Success, string? Error, Payment? Payment)> SubmitBankReceiptAsync(
        int websiteId, int clientId, int orderId, int bankAccountId, int receiptFileId, CancellationToken ct = default)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderId && o.WebsiteClientID == clientId, ct);
        if (order is null) return (false, "Order not found.", null);
        if (order.Status != (byte)OrderStatus.PendingPayment) return (false, "Order is not awaiting payment.", null);

        var payment = new Payment
        {
            WebsiteID = websiteId,
            OrderID = orderId,
            WebsiteClientID = clientId,
            Method = (byte)PaymentMethod.BankTransfer,
            BankAccountID = bankAccountId,
            ReceiptFileID = receiptFileId,
            Amount = order.GrandTotal,
            CurrencyCode = order.CurrencyCode,
            ExchangeRateToUsd = order.ExchangeRateToUsd,
            AmountUsd = order.GrandTotalUsd,
            Status = (byte)PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(ct);

        await _notifications.NotifySiteAdminsAsync(
            websiteId,
            AdminNotificationType.BankReceipt,
            "Bank receipt submitted",
            $"Order #{order.OrderNumber} needs bank transfer verification — {order.GrandTotal:0.##} {order.CurrencyCode}.",
            "/payments",
            payment.PaymentID,
            ct);

        return (true, null, payment);
    }

    public async Task<bool> VerifyAsync(int paymentId, int memberId, bool approve, string? note, CancellationToken ct = default)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentID == paymentId, ct);
        if (payment is null || payment.Status != (byte)PaymentStatus.Pending) return false;

        payment.Status = (byte)(approve ? PaymentStatus.Paid : PaymentStatus.Rejected);
        payment.VerifiedByMemberID = memberId;
        if (approve) payment.PaidAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        if (approve && payment.OrderID is int orderId)
            await _orders.TransitionStatusAsync(orderId, OrderStatus.Paid, memberId, note ?? "Bank transfer verified.", ct);

        return true;
    }

    public async Task<Payment?> GetByIdAsync(int paymentId, CancellationToken ct = default) =>
        await _context.Payments
            .Include(p => p.BankAccount).ThenInclude(a => a!.Bank)
            .Include(p => p.ReceiptFile)
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.PaymentID == paymentId, ct);

    public async Task<Payment?> GetLatestForOrderAsync(int orderId, CancellationToken ct = default) =>
        await _context.Payments.AsNoTracking()
            .Where(p => p.OrderID == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public Task<PagedResult<Payment>> GetPendingAsync(int? websiteId, GridQuery query, CancellationToken ct = default) =>
        GetManualPagedAsync(websiteId, (byte)PaymentStatus.Pending, query, ct);

    public async Task<PagedResult<Payment>> GetManualPagedAsync(int? websiteId, byte? status, GridQuery query, CancellationToken ct = default)
    {
        var q = ManualBankTransfers(websiteId);
        if (status is byte s)
            q = q.Where(p => p.Status == s);
        else if (query.GetSearch(nameof(Payment.Status)) is string statusText && byte.TryParse(statusText, out var statusByte))
            q = q.Where(p => p.Status == statusByte);

        if (query.GetSearch("OrderNumber") is string orderNumber)
            q = q.Where(p => p.Order != null && p.Order.OrderNumber.Contains(orderNumber));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Payment.CreatedAt))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);
        return new PagedResult<Payment> { Items = items, TotalCount = total };
    }

    public async Task<IReadOnlyDictionary<byte, int>> GetManualStatusCountsAsync(int? websiteId, CancellationToken ct = default)
    {
        var q = _context.Payments.AsNoTracking()
            .Where(p => p.Method == (byte)PaymentMethod.BankTransfer);
        if (websiteId is int wid) q = q.Where(p => p.WebsiteID == wid);

        var rows = await q
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.Status, r => r.Count);
    }

    private IQueryable<Payment> ManualBankTransfers(int? websiteId)
    {
        var q = _context.Payments.AsNoTracking()
            .Include(p => p.Order).Include(p => p.WebsiteClient).Include(p => p.BankAccount).ThenInclude(a => a!.Bank)
            .Include(p => p.ReceiptFile)
            .Where(p => p.Method == (byte)PaymentMethod.BankTransfer);
        if (websiteId is int wid) q = q.Where(p => p.WebsiteID == wid);
        return q;
    }

    public async Task<(bool Success, string? Error, PaymentRefund? Refund)> RefundAsync(
        int paymentId, decimal amountUsd, string? reason, bool toWallet, int? bankAccountId, int memberId, CancellationToken ct = default)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentID == paymentId, ct);
        if (payment is null || payment.Status != (byte)PaymentStatus.Paid) return (false, "Payment is not eligible for refund.", null);
        if (toWallet == (bankAccountId is not null)) return (false, "Refund must target exactly one of wallet or bank account.", null);

        int? walletTxId = null;
        if (toWallet)
        {
            var tx = await _wallet.ApplyAsync(
                payment.WebsiteID, payment.WebsiteClientID, (byte)ClientWalletTransactionType.RefundCredit, amountUsd,
                (byte)ClientWalletSourceType.PaymentRefund, paymentId, reason, memberId, ct);
            walletTxId = tx.ClientWalletTransactionID;
        }

        var refund = new PaymentRefund
        {
            PaymentID = paymentId,
            Amount = amountUsd,
            Reason = reason,
            Status = (byte)(toWallet ? PaymentRefundStatus.Completed : PaymentRefundStatus.Pending),
            BankAccountID = bankAccountId,
            ClientWalletTransactionID = walletTxId,
            RefundedAt = toWallet ? DateTime.UtcNow : null,
            CreatedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
        };
        _context.PaymentRefunds.Add(refund);

        payment.Status = (byte)PaymentStatus.Refunded;
        await _context.SaveChangesAsync(ct);

        if (payment.OrderID is int orderId)
            await _orders.TransitionStatusAsync(orderId, OrderStatus.Refunded, memberId, reason, ct);

        return (true, null, refund);
    }

    public async Task<bool> CompleteBankRefundAsync(int paymentRefundId, int memberId, CancellationToken ct = default)
    {
        var refund = await _context.PaymentRefunds.FirstOrDefaultAsync(r => r.PaymentRefundID == paymentRefundId, ct);
        if (refund is null || refund.Status != (byte)PaymentRefundStatus.Pending) return false;

        refund.Status = (byte)PaymentRefundStatus.Completed;
        refund.RefundedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public Task<PagedResult<PaymentRefund>> GetPendingRefundsAsync(int? websiteId, GridQuery query, CancellationToken ct = default) =>
        GetBankRefundsPagedAsync(websiteId, (byte)PaymentRefundStatus.Pending, query, ct);

    public async Task<PagedResult<PaymentRefund>> GetBankRefundsPagedAsync(int? websiteId, byte? status, GridQuery query, CancellationToken ct = default)
    {
        var q = BankRefunds(websiteId);
        if (status is byte s)
            q = q.Where(r => r.Status == s);
        else if (query.GetSearch(nameof(PaymentRefund.Status)) is string statusText && byte.TryParse(statusText, out var statusByte))
            q = q.Where(r => r.Status == statusByte);

        if (query.GetSearch(nameof(PaymentRefund.Reason)) is string reason)
            q = q.Where(r => r.Reason != null && r.Reason.Contains(reason));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(PaymentRefund.CreatedAt))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);
        return new PagedResult<PaymentRefund> { Items = items, TotalCount = total };
    }

    public async Task<IReadOnlyDictionary<byte, int>> GetBankRefundStatusCountsAsync(int? websiteId, CancellationToken ct = default)
    {
        var q = _context.PaymentRefunds.AsNoTracking()
            .Where(r => r.BankAccountID != null);
        if (websiteId is int wid)
            q = q.Where(r => r.Payment.WebsiteID == wid);

        var rows = await q
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.Status, r => r.Count);
    }

    private IQueryable<PaymentRefund> BankRefunds(int? websiteId)
    {
        var q = _context.PaymentRefunds.AsNoTracking()
            .Include(r => r.Payment).ThenInclude(p => p.Order)
            .Include(r => r.Payment).ThenInclude(p => p.WebsiteClient)
            .Include(r => r.BankAccount).ThenInclude(a => a!.Bank)
            .Where(r => r.BankAccountID != null);
        if (websiteId is int wid)
            q = q.Where(r => r.Payment.WebsiteID == wid);
        return q;
    }
}
