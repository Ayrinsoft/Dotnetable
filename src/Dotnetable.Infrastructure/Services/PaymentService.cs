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
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IClientWalletService _wallet;
    private readonly IOrderService _orders;
    private readonly IAdminNotificationService _notifications;
    private readonly IFinancialLedgerService _ledger;
    private readonly IStockDocumentService _stockDocs;

    public PaymentService(
        IDbContextFactory<AppDbContext> contextFactory, IClientWalletService wallet, IOrderService orders,
        IAdminNotificationService notifications, IFinancialLedgerService ledger,
        IStockDocumentService stockDocs)
    {
        _contextFactory = contextFactory;
        _wallet = wallet;
        _orders = orders;
        _notifications = notifications;
        _ledger = ledger;
        _stockDocs = stockDocs;
    }

    public async Task<(bool Success, string? Error, Payment? Payment)> PayWithWalletAsync(int websiteId, int clientId, int orderId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderId && o.WebsiteClientID == clientId, ct);
        if (order is null) return (false, "Order not found.", null);
        if (order.Status != (byte)OrderStatus.PendingPayment) return (false, "Order is not awaiting payment.", null);

        ClientWalletTransaction walletTx;
        try
        {
            // Debit the wallet that matches the order currency (separate ledger per currency).
            walletTx = await _wallet.ApplyAsync(
                websiteId, clientId, (byte)ClientWalletTransactionType.PurchaseUse, -order.GrandTotal,
                (byte)ClientWalletSourceType.Payment, orderId, $"Order {order.OrderNumber}", null,
                order.CurrencyCode, ct);
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
        try { await _ledger.PostOrderPaidBreakdownAsync(orderId, payment.PaymentID, null, ct); }
        catch { /* ledger must not block payment */ }

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
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

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

    public async Task<(bool Success, string? Error, Payment? Payment)> RecordReceivedPaymentAsync(
        int orderId, PaymentMethod method, decimal? amountLocal, string? reference, string? note,
        int memberId, int? bankAccountId = null, int? receiptFileId = null, bool markAsPaid = true,
        DateTime? paidAtUtc = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (method is not (PaymentMethod.Manual or PaymentMethod.CashOnDelivery or PaymentMethod.BankTransfer))
            return (false, "Only Manual, CashOnDelivery, or BankTransfer methods can be recorded by admin.", null);

        var order = await _context.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
        if (order is null) return (false, "Order not found.", null);

        if (order.Status is (byte)OrderStatus.Cancelled or (byte)OrderStatus.Refunded)
            return (false, "Cannot record payment on a cancelled or refunded order.", null);

        if (order.Payments.Any(p => p.Status == (byte)PaymentStatus.Paid))
            return (false, "Order already has a paid payment recorded.", null);

        if (method == PaymentMethod.BankTransfer && bankAccountId is int baId)
        {
            var accountOk = await _context.BankAccounts.AnyAsync(
                a => a.BankAccountID == baId && a.WebsiteID == order.WebsiteID && a.IsActive, ct);
            if (!accountOk)
                return (false, "Bank account not found or inactive for this website.", null);
        }
        else if (method == PaymentMethod.BankTransfer && bankAccountId is null)
        {
            // Bank account optional but recommended; allow null for free-form card-to-card notes.
        }

        if (receiptFileId is int fileId)
        {
            var fileOk = await _context.FileRecords.AnyAsync(
                f => f.FileRecordID == fileId && f.WebsiteID == order.WebsiteID && !f.IsDeleted, ct);
            if (!fileOk)
                return (false, "Receipt file not found for this website.", null);
        }

        var amount = amountLocal is > 0 ? amountLocal.Value : order.GrandTotal;
        if (amount <= 0) return (false, "Amount must be greater than zero.", null);

        var rate = order.ExchangeRateToUsd <= 0 ? 1m : order.ExchangeRateToUsd;
        var amountUsd = Math.Round(amount / rate, 4, MidpointRounding.AwayFromZero);

        // Bank transfer can stay Pending for the queue when markAsPaid is false; other methods are always Paid.
        var paid = markAsPaid || method is not PaymentMethod.BankTransfer;
        var effectivePaidAt = paid
            ? (paidAtUtc.HasValue
                ? (paidAtUtc.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(paidAtUtc.Value, DateTimeKind.Utc)
                    : paidAtUtc.Value.ToUniversalTime())
                : DateTime.UtcNow)
            : (DateTime?)null;

        var payment = new Payment
        {
            WebsiteID = order.WebsiteID,
            OrderID = order.OrderID,
            WebsiteClientID = order.WebsiteClientID,
            Method = (byte)method,
            BankAccountID = bankAccountId,
            ReceiptFileID = receiptFileId,
            Amount = amount,
            CurrencyCode = order.CurrencyCode,
            ExchangeRateToUsd = rate,
            AmountUsd = amountUsd,
            Status = (byte)(paid ? PaymentStatus.Paid : PaymentStatus.Pending),
            TrackingCode = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim(),
            GatewayRefNumber = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            PaidAt = effectivePaidAt,
            CreatedByMemberID = memberId,
            VerifiedByMemberID = paid ? memberId : null,
            CreatedAt = DateTime.UtcNow,
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(ct);

        if (paid && order.Status == (byte)OrderStatus.PendingPayment)
        {
            var transitionNote = string.IsNullOrWhiteSpace(note)
                ? $"Payment received ({method})."
                : note.Trim();
            await _orders.TransitionStatusAsync(orderId, OrderStatus.Paid, memberId, transitionNote, ct);

            // Align order.PaidAt with the admin-entered payment time (transition may have set UtcNow).
            if (effectivePaidAt is DateTime at)
            {
                var trackedOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
                if (trackedOrder is not null)
                {
                    trackedOrder.PaidAt = at;
                    await _context.SaveChangesAsync(ct);
                }
            }

            try { await _ledger.PostOrderPaidBreakdownAsync(orderId, payment.PaymentID, memberId, ct); }
            catch { /* ledger must not block payment */ }
        }
        else if (paid)
        {
            // Order already Paid (e.g. additional recording path) — still refresh ledger.
            try { await _ledger.PostOrderPaidBreakdownAsync(orderId, payment.PaymentID, memberId, ct); }
            catch { /* ignore */ }
        }

        await _notifications.NotifySiteAdminsAsync(
            order.WebsiteID,
            paid ? AdminNotificationType.PaymentReceived : AdminNotificationType.BankReceipt,
            paid ? "Payment recorded" : "Bank receipt recorded",
            paid
                ? $"Order #{order.OrderNumber}: admin recorded {method} payment — {amount:0.##} {order.CurrencyCode}."
                : $"Order #{order.OrderNumber}: admin submitted bank receipt for verification — {amount:0.##} {order.CurrencyCode}.",
            $"/orders/{order.OrderID}",
            payment.PaymentID,
            ct);

        return (true, null, payment);
    }

    public async Task<(bool Success, string? Error, Payment? Payment)> RecordWalletDepositAsync(
        int websiteId, int clientId, decimal amount, string? currencyCode,
        int? bankAccountId, string? description, string? reference, int? receiptFileId,
        int memberId, DateTime? paidAtUtc = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (amount <= 0) return (false, "Amount must be greater than zero.", null);
        if (string.IsNullOrWhiteSpace(description))
            return (false, "Payment description is required.", null);

        var client = await _context.WebsiteClients.AsNoTracking()
            .FirstOrDefaultAsync(c => c.WebsiteClientID == clientId && c.WebsiteID == websiteId, ct);
        if (client is null) return (false, "Customer not found.", null);

        var website = await _context.Websites.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebsiteID == websiteId, ct);
        if (website is null) return (false, "Website not found.", null);

        var code = string.IsNullOrWhiteSpace(currencyCode)
            ? website.DefaultCurrencyCode
            : currencyCode.Trim().ToUpperInvariant();
        if (code.Length > 3) code = code[..3];

        if (bankAccountId is int baId)
        {
            var accountOk = await _context.BankAccounts.AnyAsync(
                a => a.BankAccountID == baId && a.WebsiteID == websiteId && a.IsActive, ct);
            if (!accountOk)
                return (false, "Bank account not found or inactive for this website.", null);
        }

        if (receiptFileId is int fileId)
        {
            var fileOk = await _context.FileRecords.AnyAsync(
                f => f.FileRecordID == fileId && f.WebsiteID == websiteId && !f.IsDeleted, ct);
            if (!fileOk)
                return (false, "Receipt file not found for this website.", null);
        }

        var desc = description.Trim();
        if (desc.Length > 100) desc = desc[..100];
        var paidAt = paidAtUtc.HasValue
            ? (paidAtUtc.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(paidAtUtc.Value, DateTimeKind.Utc)
                : paidAtUtc.Value.ToUniversalTime())
            : DateTime.UtcNow;

        ClientWalletTransaction walletTx;
        try
        {
            walletTx = await _wallet.ApplyAsync(
                websiteId, clientId, (byte)ClientWalletTransactionType.AdminDeposit, amount,
                (byte)ClientWalletSourceType.Payment, null, desc, memberId, code, ct);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message, null);
        }

        var rate = 1m;
        var amountUsd = string.Equals(code, "USD", StringComparison.OrdinalIgnoreCase) ? amount : 0m;

        var payment = new Payment
        {
            WebsiteID = websiteId,
            OrderID = null,
            WebsiteClientID = clientId,
            Method = (byte)PaymentMethod.BankTransfer,
            BankAccountID = bankAccountId,
            ReceiptFileID = receiptFileId,
            ClientWalletTransactionID = walletTx.ClientWalletTransactionID,
            Amount = amount,
            CurrencyCode = code,
            ExchangeRateToUsd = rate,
            AmountUsd = amountUsd,
            Status = (byte)PaymentStatus.Paid,
            TrackingCode = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim(),
            GatewayRefNumber = desc,
            PaidAt = paidAt,
            CreatedByMemberID = memberId,
            VerifiedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(ct);

        walletTx.SourceId = payment.PaymentID;
        await _context.SaveChangesAsync(ct);

        return (true, null, payment);
    }

    public async Task<bool> VerifyAsync(int paymentId, int memberId, bool approve, string? note, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentID == paymentId, ct);
        if (payment is null || payment.Status != (byte)PaymentStatus.Pending) return false;

        payment.Status = (byte)(approve ? PaymentStatus.Paid : PaymentStatus.Rejected);
        payment.VerifiedByMemberID = memberId;
        if (approve) payment.PaidAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        if (approve && payment.OrderID is int orderId)
        {
            await _orders.TransitionStatusAsync(orderId, OrderStatus.Paid, memberId, note ?? "Bank transfer verified.", ct);
            try { await _ledger.PostOrderPaidBreakdownAsync(orderId, payment.PaymentID, memberId, ct); }
            catch { /* ignore */ }
        }
        else if (approve && payment.OrderID is null && payment.ClientWalletTransactionID is null)
        {
            try
            {
                var tx = await _wallet.ApplyAsync(
                    payment.WebsiteID, payment.WebsiteClientID,
                    (byte)ClientWalletTransactionType.AdminDeposit, payment.Amount,
                    (byte)ClientWalletSourceType.Payment, payment.PaymentID,
                    note ?? payment.GatewayRefNumber, memberId, payment.CurrencyCode, ct);
                payment.ClientWalletTransactionID = tx.ClientWalletTransactionID;
                await _context.SaveChangesAsync(ct);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        return true;
    }

    public async Task<Payment?> GetByIdAsync(int paymentId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.Payments
            .Include(p => p.BankAccount).ThenInclude(a => a!.Bank)
            .Include(p => p.ReceiptFile)
            .Include(p => p.Order)
            .Include(p => p.WebsiteClient)
            .Include(p => p.CreatedByMember)
            .Include(p => p.VerifiedByMember)
            .FirstOrDefaultAsync(p => p.PaymentID == paymentId, ct);
    }

    public async Task<Payment?> GetLatestForOrderAsync(int orderId, int? clientId = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.Payments.AsNoTracking()
            .Where(p => p.OrderID == orderId && (clientId == null || p.WebsiteClientID == clientId))
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public Task<PagedResult<Payment>> GetPendingAsync(int? websiteId, GridQuery query, CancellationToken ct = default) =>
        GetManualPagedAsync(websiteId, (byte)PaymentStatus.Pending, query, ct);

    public async Task<PagedResult<Payment>> GetManualPagedAsync(int? websiteId, byte? status, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = ManualBankTransfers(_context, websiteId);
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
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.Payments.AsNoTracking()
            .Where(p => p.Method == (byte)PaymentMethod.BankTransfer
                        || (p.OrderID == null && p.CreatedByMemberID != null));
        if (websiteId is int wid) q = q.Where(p => p.WebsiteID == wid);

        var rows = await q
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.Status, r => r.Count);
    }

    // Takes the caller's context: an IQueryable is only usable while the context that built it is alive.
    private static IQueryable<Payment> ManualBankTransfers(AppDbContext _context, int? websiteId)
    {
        var q = _context.Payments.AsNoTracking()
            .Include(p => p.Order)
            .Include(p => p.WebsiteClient)
            .Include(p => p.BankAccount).ThenInclude(a => a!.Bank)
            .Include(p => p.ReceiptFile)
            .Include(p => p.CreatedByMember)
            .Include(p => p.VerifiedByMember)
            .Where(p => p.Method == (byte)PaymentMethod.BankTransfer
                        || (p.OrderID == null && p.CreatedByMemberID != null));
        if (websiteId is int wid) q = q.Where(p => p.WebsiteID == wid);
        return q;
    }

    public async Task<(bool Success, string? Error, PaymentRefund? Refund)> RefundAsync(
        int paymentId, decimal amount, string? reason, bool toWallet, int? bankAccountId, int memberId,
        bool markCompleted = false, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentID == paymentId, ct);
        if (payment is null) return (false, "Payment is not eligible for refund.", null);
        if (payment.Status is not ((byte)PaymentStatus.Paid or (byte)PaymentStatus.Refunded))
            return (false, "Payment is not eligible for refund.", null);
        if (amount <= 0) return (false, "Refund amount must be greater than zero.", null);

        var alreadyRefunded = await _context.PaymentRefunds.AsNoTracking()
            .Where(r => r.PaymentID == paymentId)
            .SumAsync(r => (decimal?)r.Amount, ct) ?? 0m;
        var remaining = payment.Amount - alreadyRefunded;
        if (remaining <= 0)
            return (false, "Payment has already been fully refunded.", null);
        if (amount > remaining)
            return (false, $"Refund amount cannot exceed the remaining paid amount ({remaining:0.####} {payment.CurrencyCode}).", null);

        if (toWallet && bankAccountId is not null)
            return (false, "Choose wallet, bank account, or cash/manual — not wallet and bank together.", null);

        if (bankAccountId is int baId)
        {
            var accountOk = await _context.BankAccounts.AnyAsync(
                a => a.BankAccountID == baId && a.WebsiteID == payment.WebsiteID && a.IsActive, ct);
            if (!accountOk)
                return (false, "Bank account not found or inactive for this website.", null);
        }

        // Destination: wallet (instant), bank (pending unless already transferred), or cash/manual (instant).
        var isCashManual = !toWallet && bankAccountId is null;
        var completedNow = toWallet || isCashManual || (bankAccountId is not null && markCompleted);

        int? walletTxId = null;
        if (toWallet)
        {
            var tx = await _wallet.ApplyAsync(
                payment.WebsiteID, payment.WebsiteClientID, (byte)ClientWalletTransactionType.RefundCredit, amount,
                (byte)ClientWalletSourceType.PaymentRefund, paymentId, reason, memberId,
                payment.CurrencyCode, ct);
            walletTxId = tx.ClientWalletTransactionID;
        }

        var refund = new PaymentRefund
        {
            PaymentID = paymentId,
            Amount = amount,
            Reason = reason,
            Status = (byte)(completedNow ? PaymentRefundStatus.Completed : PaymentRefundStatus.Pending),
            BankAccountID = bankAccountId,
            ClientWalletTransactionID = walletTxId,
            RefundedAt = completedNow ? DateTime.UtcNow : null,
            CreatedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
        };
        _context.PaymentRefunds.Add(refund);

        var fullyRefunded = amount >= remaining;
        if (fullyRefunded)
            payment.Status = (byte)PaymentStatus.Refunded;
        await _context.SaveChangesAsync(ct);

        if (payment.OrderID is int orderId)
        {
            if (fullyRefunded)
                await _orders.TransitionStatusAsync(orderId, OrderStatus.Refunded, memberId, reason, ct);

            // After goods left stock: auto create Return document for warehouse QC + restock.
            try
            {
                await _stockDocs.EnsureReturnForRefundAsync(orderId, refund.PaymentRefundID, memberId, ct);
            }
            catch
            {
                /* stock return is best-effort after money path; warehouse can create manually */
            }

            if (completedNow)
            {
                try { await _ledger.PostCustomerRefundAsync(orderId, paymentId, amount, reason, memberId, ct); }
                catch { /* ignore */ }
            }
        }

        return (true, null, refund);
    }

    public async Task<IReadOnlyList<RefundablePaymentDto>> GetRefundablePaymentsAsync(
        int websiteId, int clientId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var rows = await _context.Payments.AsNoTracking()
            .Where(p => p.WebsiteID == websiteId
                        && p.WebsiteClientID == clientId
                        && (p.Status == (byte)PaymentStatus.Paid || p.Status == (byte)PaymentStatus.Refunded))
            .Select(p => new
            {
                p.PaymentID,
                p.OrderID,
                OrderNumber = p.Order != null ? p.Order.OrderNumber : null,
                p.CurrencyCode,
                p.Amount,
                RefundedAmount = p.PaymentRefunds.Sum(r => (decimal?)r.Amount) ?? 0m,
                p.CreatedAt,
            })
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        return rows
            .Select(p => new RefundablePaymentDto
            {
                PaymentID = p.PaymentID,
                OrderID = p.OrderID,
                OrderNumber = p.OrderNumber,
                CurrencyCode = p.CurrencyCode,
                Amount = p.Amount,
                RefundedAmount = p.RefundedAmount,
                RemainingAmount = p.Amount - p.RefundedAmount,
                CreatedAt = p.CreatedAt,
            })
            .Where(p => p.RemainingAmount > 0)
            .ToList();
    }

    public async Task<bool> CompleteBankRefundAsync(int paymentRefundId, int memberId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var refund = await _context.PaymentRefunds.FirstOrDefaultAsync(r => r.PaymentRefundID == paymentRefundId, ct);
        if (refund is null || refund.Status != (byte)PaymentRefundStatus.Pending) return false;

        refund.Status = (byte)PaymentRefundStatus.Completed;
        refund.RefundedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        var payment = await _context.Payments.AsNoTracking()
            .FirstOrDefaultAsync(p => p.PaymentID == refund.PaymentID, ct);
        if (payment?.OrderID is int orderId)
        {
            try { await _ledger.PostCustomerRefundAsync(orderId, payment.PaymentID, refund.Amount, refund.Reason, memberId, ct); }
            catch { /* ignore */ }
        }

        return true;
    }

    public Task<PagedResult<PaymentRefund>> GetPendingRefundsAsync(int? websiteId, GridQuery query, CancellationToken ct = default) =>
        GetBankRefundsPagedAsync(websiteId, (byte)PaymentRefundStatus.Pending, query, ct);

    public async Task<PagedResult<PaymentRefund>> GetBankRefundsPagedAsync(int? websiteId, byte? status, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = BankRefunds(_context, websiteId);
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
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

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

    // Takes the caller's context: an IQueryable is only usable while the context that built it is alive.
    private static IQueryable<PaymentRefund> BankRefunds(AppDbContext _context, int? websiteId)
    {
        var q = _context.PaymentRefunds.AsNoTracking()
            .Include(r => r.Payment).ThenInclude(p => p.Order)
            .Include(r => r.Payment).ThenInclude(p => p.WebsiteClient)
            .Include(r => r.BankAccount).ThenInclude(a => a!.Bank)
            .Include(r => r.CreatedByMember)
            .Where(r => r.BankAccountID != null);
        if (websiteId is int wid)
            q = q.Where(r => r.Payment.WebsiteID == wid);
        return q;
    }
}
