using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class VendorCreditService : IVendorCreditService
{
    private readonly AppDbContext _context;
    private readonly IVendorService _vendors;

    public VendorCreditService(AppDbContext context, IVendorService vendors)
    {
        _context = context;
        _vendors = vendors;
    }

    public async Task<VendorCreditBalanceDto> GetBalanceAsync(int vendorId, CancellationToken ct = default)
    {
        var vendor = await _context.Vendors.AsNoTracking().FirstOrDefaultAsync(v => v.VendorID == vendorId, ct)
                     ?? throw new InvalidOperationException("Vendor not found.");
        return new VendorCreditBalanceDto
        {
            VendorID = vendor.VendorID,
            AvailableCreditUsd = vendor.AvailableCreditUsd,
            CreditLimitUsd = vendor.CreditLimitUsd,
            SettlementMode = vendor.SettlementMode,
            VendorType = vendor.VendorType,
            LinkedWebsiteID = vendor.LinkedWebsiteID,
            CanExposeCatalog = _vendors.CanExposeCatalog(vendor),
        };
    }

    public async Task<(bool Success, string? Error, VendorCreditTransaction? Tx)> GrantAsync(
        int vendorId, decimal amountUsd, string? note, int? memberId, CancellationToken ct = default)
    {
        if (amountUsd == 0) return (false, "Amount must be non-zero.", null);

        var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.VendorID == vendorId, ct);
        if (vendor is null) return (false, "Vendor not found.", null);

        vendor.AvailableCreditUsd += amountUsd;
        if (vendor.AvailableCreditUsd < 0)
            return (false, "Resulting credit balance cannot be negative.", null);

        if (vendor.CreditLimitUsd is null || amountUsd > 0)
            vendor.CreditLimitUsd = Math.Max(vendor.CreditLimitUsd ?? 0, vendor.AvailableCreditUsd);

        var tx = new VendorCreditTransaction
        {
            VendorID = vendor.VendorID,
            WebsiteID = vendor.WebsiteID,
            AmountUsd = amountUsd,
            BalanceAfterUsd = vendor.AvailableCreditUsd,
            SourceType = amountUsd > 0 ? (byte)VendorCreditSourceType.Grant : (byte)VendorCreditSourceType.Adjustment,
            Note = note,
            CreatedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
        };
        _context.VendorCreditTransactions.Add(tx);
        await _context.SaveChangesAsync(ct);
        return (true, null, tx);
    }

    public async Task<PagedResult<VendorCreditTransaction>> GetHistoryAsync(int vendorId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.VendorCreditTransactions.AsNoTracking()
            .Where(t => t.VendorID == vendorId);
        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(VendorCreditTransaction.CreatedAt), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);
        return new PagedResult<VendorCreditTransaction> { Items = items, TotalCount = total };
    }

    public async Task SettleHostOrderAsync(int hostOrderId, CancellationToken ct = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderID == hostOrderId, ct);
        if (order is null) return;

        var vendorLines = order.OrderItems
            .Where(i => i.VendorID is int && i.VendorProductID is int)
            .ToList();
        if (vendorLines.Count == 0) return;

        // Already settled?
        var already = await _context.VendorCreditTransactions.AsNoTracking()
            .AnyAsync(t => t.SourceOrderItemID != null
                           && vendorLines.Select(i => i.OrderItemID).Contains(t.SourceOrderItemID.Value)
                           && t.SourceType == (byte)VendorCreditSourceType.Sale, ct);
        if (already) return;

        var vendorIds = vendorLines.Select(i => i.VendorID!.Value).Distinct().ToList();
        var vendors = await _context.Vendors
            .Where(v => vendorIds.Contains(v.VendorID))
            .ToDictionaryAsync(v => v.VendorID, ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        foreach (var group in vendorLines.GroupBy(i => i.VendorID!.Value))
        {
            if (!vendors.TryGetValue(group.Key, out var vendor)) continue;

            var amountUsd = group.Sum(i => i.UnitPriceUsd * i.Quantity - i.DiscountAmount);
            if (amountUsd < 0) amountUsd = 0;

            // Credit-mode site vendors: require and debit available credit.
            if (vendor.VendorType == (byte)VendorType.Site && vendor.SettlementMode == 1)
            {
                if (vendor.AvailableCreditUsd < amountUsd)
                    throw new InvalidOperationException(
                        $"Insufficient inter-site credit for vendor '{vendor.Name}'. Need {amountUsd:0.####} USD, have {vendor.AvailableCreditUsd:0.####}.");

                vendor.AvailableCreditUsd -= amountUsd;
                _context.VendorCreditTransactions.Add(new VendorCreditTransaction
                {
                    VendorID = vendor.VendorID,
                    WebsiteID = order.WebsiteID,
                    AmountUsd = -amountUsd,
                    BalanceAfterUsd = vendor.AvailableCreditUsd,
                    SourceType = (byte)VendorCreditSourceType.Sale,
                    SourceOrderItemID = group.First().OrderItemID,
                    Note = $"Sale against host order {order.OrderNumber}",
                    CreatedAt = DateTime.UtcNow,
                });
            }
            else if (vendor.VendorType == (byte)VendorType.Site)
            {
                // Immediate mode still records a zero-impact ledger note via settlement only.
            }

            // Dual settlements: host pays source (or vendor), and source records receivable from host.
            var hostSettlement = new Settlement
            {
                WebsiteID = order.WebsiteID,
                TargetType = vendor.VendorType == (byte)VendorType.Site
                    ? (byte)SettlementTargetType.Website
                    : (byte)SettlementTargetType.Vendor,
                VendorID = vendor.VendorID,
                TargetWebsiteID = vendor.LinkedWebsiteID,
                PeriodFrom = today,
                PeriodTo = today,
                TotalAmount = amountUsd,
                CurrencyCode = "USD",
                Status = 1,
                Note = $"Host order {order.OrderNumber}",
                CreatedAt = DateTime.UtcNow,
            };
            _context.Settlements.Add(hostSettlement);
            await _context.SaveChangesAsync(ct);

            foreach (var line in group)
            {
                _context.SettlementItems.Add(new SettlementItem
                {
                    SettlementID = hostSettlement.SettlementID,
                    OrderItemID = line.OrderItemID,
                    Amount = line.UnitPriceUsd * line.Quantity - line.DiscountAmount,
                    Description = line.TitleSnapshot,
                });
            }

            if (vendor.VendorType == (byte)VendorType.Site && vendor.LinkedWebsiteID is int sourceWebsiteId)
            {
                var mirrorOrderId = await CreateMirrorOrderAsync(order, group.ToList(), sourceWebsiteId, vendor, ct);

                var sourceSettlement = new Settlement
                {
                    WebsiteID = sourceWebsiteId,
                    TargetType = (byte)SettlementTargetType.Website,
                    TargetWebsiteID = order.WebsiteID,
                    VendorID = null,
                    PeriodFrom = today,
                    PeriodTo = today,
                    TotalAmount = amountUsd,
                    CurrencyCode = "USD",
                    Status = 1,
                    Note = $"Mirror of host order {order.OrderNumber} (host site {order.WebsiteID})",
                    CreatedAt = DateTime.UtcNow,
                };
                _context.Settlements.Add(sourceSettlement);
                await _context.SaveChangesAsync(ct);

                // Link sale ledger row to mirror order when we debited credit.
                var saleTx = await _context.VendorCreditTransactions
                    .OrderByDescending(t => t.VendorCreditTransactionID)
                    .FirstOrDefaultAsync(t => t.VendorID == vendor.VendorID
                                              && t.SourceType == (byte)VendorCreditSourceType.Sale
                                              && t.SourceOrderItemID == group.First().OrderItemID
                                              && t.MirrorOrderID == null, ct);
                if (saleTx is not null)
                    saleTx.MirrorOrderID = mirrorOrderId;

                foreach (var line in group)
                {
                    _context.SettlementItems.Add(new SettlementItem
                    {
                        SettlementID = sourceSettlement.SettlementID,
                        OrderItemID = line.OrderItemID,
                        Amount = line.UnitPriceUsd * line.Quantity - line.DiscountAmount,
                        Description = $"{line.TitleSnapshot} (from host {order.OrderNumber})",
                    });
                }
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task ReverseHostOrderAsync(int hostOrderId, CancellationToken ct = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderID == hostOrderId, ct);
        if (order is null) return;

        var itemIds = order.OrderItems.Select(i => i.OrderItemID).ToList();
        var sales = await _context.VendorCreditTransactions
            .Where(t => t.SourceOrderItemID != null
                        && itemIds.Contains(t.SourceOrderItemID.Value)
                        && t.SourceType == (byte)VendorCreditSourceType.Sale)
            .ToListAsync(ct);
        if (sales.Count == 0) return;

        // Avoid double refund.
        var alreadyRefunded = await _context.VendorCreditTransactions.AsNoTracking()
            .AnyAsync(t => t.SourceOrderItemID != null
                           && itemIds.Contains(t.SourceOrderItemID.Value)
                           && t.SourceType == (byte)VendorCreditSourceType.Refund, ct);
        if (alreadyRefunded) return;

        foreach (var sale in sales)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.VendorID == sale.VendorID, ct);
            if (vendor is null) continue;

            var restore = -sale.AmountUsd; // sale amount is negative
            vendor.AvailableCreditUsd += restore;
            _context.VendorCreditTransactions.Add(new VendorCreditTransaction
            {
                VendorID = vendor.VendorID,
                WebsiteID = vendor.WebsiteID,
                AmountUsd = restore,
                BalanceAfterUsd = vendor.AvailableCreditUsd,
                SourceType = (byte)VendorCreditSourceType.Refund,
                SourceOrderItemID = sale.SourceOrderItemID,
                MirrorOrderID = sale.MirrorOrderID,
                Note = $"Refund host order {order.OrderNumber}",
                CreatedAt = DateTime.UtcNow,
            });

            if (sale.MirrorOrderID is int mid)
            {
                var mirror = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == mid, ct);
                if (mirror is not null && mirror.Status != (byte)OrderStatus.Cancelled && mirror.Status != (byte)OrderStatus.Refunded)
                    mirror.Status = (byte)OrderStatus.Cancelled;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Creates a mirror order on the source website so both sites see a purchase.
    /// Uses a synthetic inter-site system client; buyer wallet stays on the host only.
    /// </summary>
    private async Task<int> CreateMirrorOrderAsync(
        Order hostOrder, List<OrderItem> lines, int sourceWebsiteId, Vendor vendor, CancellationToken ct)
    {
        var client = await GetOrCreateInterSiteClientAsync(sourceWebsiteId, hostOrder.WebsiteID, ct);
        var amountUsd = lines.Sum(i => i.UnitPriceUsd * i.Quantity - i.DiscountAmount);

        var mirror = new Order
        {
            WebsiteID = sourceWebsiteId,
            OrderNumber = $"M{hostOrder.WebsiteID}-{hostOrder.OrderNumber}",
            WebsiteClientID = client.WebsiteClientID,
            Status = (byte)OrderStatus.Paid,
            CurrencyCode = "USD",
            ExchangeRateToUsd = 1,
            SubTotal = amountUsd,
            DiscountTotal = 0,
            ShippingTotal = 0,
            TaxTotal = 0,
            GrandTotal = amountUsd,
            GrandTotalUsd = amountUsd,
            AddressSnapshot = hostOrder.AddressSnapshot,
            Note = $"Inter-site sale via host order {hostOrder.OrderNumber} (vendor {vendor.Name})",
            CreatedAt = DateTime.UtcNow,
            PaidAt = DateTime.UtcNow,
        };
        _context.Orders.Add(mirror);
        await _context.SaveChangesAsync(ct);

        foreach (var line in lines)
        {
            _context.OrderItems.Add(new OrderItem
            {
                OrderID = mirror.OrderID,
                WebsiteID = sourceWebsiteId,
                SourceWebsiteID = sourceWebsiteId,
                ProductVariantID = line.ProductVariantID,
                VendorProductID = null,
                VendorID = null,
                TitleSnapshot = line.TitleSnapshot,
                SkuSnapshot = line.SkuSnapshot,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPriceUsd,
                UnitPriceUsd = line.UnitPriceUsd,
                UnitCostUsd = line.UnitCostUsd,
                DiscountAmount = line.DiscountAmount,
                TotalPrice = line.UnitPriceUsd * line.Quantity - line.DiscountAmount,
            });
        }

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderID = mirror.OrderID,
            FromStatus = null,
            ToStatus = (byte)OrderStatus.Paid,
            Note = $"Mirror of host order {hostOrder.OrderID}",
            CreatedAt = DateTime.UtcNow,
        });

        await _context.SaveChangesAsync(ct);
        return mirror.OrderID;
    }

    private async Task<WebsiteClient> GetOrCreateInterSiteClientAsync(int sourceWebsiteId, int hostWebsiteId, CancellationToken ct)
    {
        var email = $"intersite+host{hostWebsiteId}@system.local";
        var client = await _context.WebsiteClients
            .FirstOrDefaultAsync(c => c.WebsiteID == sourceWebsiteId && c.Email == email, ct);
        if (client is not null) return client;

        client = new WebsiteClient
        {
            WebsiteID = sourceWebsiteId,
            Email = email,
            Active = true,
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Givenname = "Inter-site",
            Surname = $"Host-{hostWebsiteId}",
            HashKey = Guid.NewGuid(),
            ClientLevel = 0,
        };
        _context.WebsiteClients.Add(client);
        await _context.SaveChangesAsync(ct);
        return client;
    }
}
