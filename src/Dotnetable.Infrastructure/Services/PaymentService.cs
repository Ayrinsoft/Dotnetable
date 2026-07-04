using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly IClientWalletService _wallet;
    private readonly IOrderService _orders;

    public PaymentService(AppDbContext context, IClientWalletService wallet, IOrderService orders)
    {
        _context = context;
        _wallet = wallet;
        _orders = orders;
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

    public async Task<PagedResult<Payment>> GetPendingAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Payments.AsNoTracking()
            .Include(p => p.Order).Include(p => p.WebsiteClient).Include(p => p.BankAccount).ThenInclude(a => a!.Bank)
            .Include(p => p.ReceiptFile)
            .Where(p => p.Status == (byte)PaymentStatus.Pending);
        if (websiteId is int wid) q = q.Where(p => p.WebsiteID == wid);

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(p => p.CreatedAt).Skip(query.Skip).Take(query.Take).ToListAsync(ct);
        return new PagedResult<Payment> { Items = items, TotalCount = total };
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

    public async Task<PagedResult<PaymentRefund>> GetPendingRefundsAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.PaymentRefunds.AsNoTracking()
            .Include(r => r.Payment).ThenInclude(p => p.Order)
            .Include(r => r.Payment).ThenInclude(p => p.WebsiteClient)
            .Include(r => r.BankAccount).ThenInclude(a => a!.Bank)
            .Where(r => r.Status == (byte)PaymentRefundStatus.Pending && r.BankAccountID != null);
        if (websiteId is int wid) q = q.Where(r => r.Payment.WebsiteID == wid);

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(r => r.CreatedAt).Skip(query.Skip).Take(query.Take).ToListAsync(ct);
        return new PagedResult<PaymentRefund> { Items = items, TotalCount = total };
    }
}
