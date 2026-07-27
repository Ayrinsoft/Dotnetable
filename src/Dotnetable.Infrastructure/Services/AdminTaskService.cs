using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class AdminTaskService : IAdminTaskService
{
    private readonly AppDbContext _context;

    public AdminTaskService(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<AdminOpenTask>> GetOpenTasksAsync(
        int? websiteId,
        bool includeOrders,
        bool includePayments,
        bool includeRefunds,
        bool includeWithdrawals,
        CancellationToken ct = default)
    {
        var tasks = new List<AdminOpenTask>();

        if (includePayments)
        {
            var q = _context.Payments.AsNoTracking()
                .Where(p => p.Method == (byte)PaymentMethod.BankTransfer
                            && p.Status == (byte)PaymentStatus.Pending);
            if (websiteId is int wid)
                q = q.Where(p => p.WebsiteID == wid);

            var count = await q.CountAsync(ct);
            if (count > 0)
                tasks.Add(new AdminOpenTask("payments.pending", count, "/payments"));
        }

        if (includeRefunds)
        {
            var q = _context.PaymentRefunds.AsNoTracking()
                .Where(r => r.BankAccountID != null
                            && r.Status == (byte)PaymentRefundStatus.Pending);
            if (websiteId is int wid)
                q = q.Where(r => r.Payment.WebsiteID == wid);

            var count = await q.CountAsync(ct);
            if (count > 0)
                tasks.Add(new AdminOpenTask("payments.refunds.pending", count, "/payments/refunds"));
        }

        if (includeWithdrawals)
        {
            var q = _context.ClientWalletWithdrawals.AsNoTracking()
                .Where(w => w.Status == (byte)ClientWalletWithdrawalStatus.Pending);
            if (websiteId is int wid)
                q = q.Where(w => w.WebsiteID == wid);

            var count = await q.CountAsync(ct);
            if (count > 0)
                tasks.Add(new AdminOpenTask("wallets.withdrawals.pending", count, "/wallets/withdrawals"));
        }

        if (includeOrders)
        {
            // Fulfillment pipeline — payment confirmation is covered by the payments task.
            byte[] fulfillment =
            [
                (byte)OrderStatus.Paid,
                (byte)OrderStatus.Processing,
                (byte)OrderStatus.Shipped,
            ];

            var q = _context.Orders.AsNoTracking()
                .Where(o => fulfillment.Contains(o.Status));
            if (websiteId is int wid)
                q = q.Where(o => o.WebsiteID == wid);

            var count = await q.CountAsync(ct);
            if (count > 0)
                tasks.Add(new AdminOpenTask("orders.fulfillment", count, "/orders"));

            // Still useful: unpaid orders waiting on the customer (or a bank receipt).
            var pendingPayQ = _context.Orders.AsNoTracking()
                .Where(o => o.Status == (byte)OrderStatus.PendingPayment);
            if (websiteId is int wid2)
                pendingPayQ = pendingPayQ.Where(o => o.WebsiteID == wid2);

            var pendingPayCount = await pendingPayQ.CountAsync(ct);
            if (pendingPayCount > 0)
                tasks.Add(new AdminOpenTask("orders.pending_payment", pendingPayCount, "/orders"));
        }

        return tasks;
    }
}
