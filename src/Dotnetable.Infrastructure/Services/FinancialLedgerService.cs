using Dotnetable.Application.DTOs;
using Dotnetable.Application.Financial;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class FinancialLedgerService : IFinancialLedgerService
{
    private readonly AppDbContext _fallback;
    private AppDbContext _context => AmbientDbContext.Current ?? _fallback;
    private readonly IGlProjector _gl;

    public FinancialLedgerService(AppDbContext context, IGlProjector gl)
    {
        _fallback = context;
        _gl = gl;
    }

    public async Task<PagedResult<FinancialLedgerEntryDto>> GetPagedAsync(
        FinancialLedgerFilter filter, GridQuery query, CancellationToken ct = default)
    {
        var q = ApplyFilter(_context.FinancialLedgerEntries.AsNoTracking(), filter);
        var total = await q.CountAsync(ct);
        var items = await q
            .Include(e => e.Vendor)
            .Include(e => e.Order)
            .OrderByDescending(e => e.OccurredDate)
            .ThenByDescending(e => e.OccurredTime)
            .ThenByDescending(e => e.FinancialLedgerEntryID)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);
        return new PagedResult<FinancialLedgerEntryDto>
        {
            Items = items.Select(MapDto).ToList(),
            TotalCount = total,
        };
    }

    public async Task<IReadOnlyList<FinancialLedgerEntryDto>> GetByOrderAsync(
        int orderId, bool currentOnly = true, bool includeHistory = false, CancellationToken ct = default)
    {
        var q = _context.FinancialLedgerEntries.AsNoTracking()
            .Include(e => e.Vendor)
            .Include(e => e.Order)
            .Where(e => e.OrderID == orderId);
        if (currentOnly && !includeHistory)
            q = q.Where(e => e.IsCurrent);
        else if (!includeHistory)
            q = q.Where(e => e.IsCurrent);

        var items = await q
            .OrderBy(e => e.OccurredDate).ThenBy(e => e.OccurredTime).ThenBy(e => e.FinancialLedgerEntryID)
            .ToListAsync(ct);

        if (includeHistory)
            return items.Select(MapDto).ToList();

        return items.Where(e => e.IsCurrent).Select(MapDto).ToList();
    }

    public async Task<OrderFinancialSummaryDto?> GetOrderSummaryAsync(int orderId, CancellationToken ct = default)
    {
        var order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
        if (order is null) return null;

        var all = await _context.FinancialLedgerEntries.AsNoTracking()
            .Include(e => e.Vendor)
            .Include(e => e.Order)
            .Where(e => e.OrderID == orderId)
            .OrderByDescending(e => e.OccurredAtUtc)
            .ThenByDescending(e => e.FinancialLedgerEntryID)
            .ToListAsync(ct);

        var current = all.Where(e => e.IsCurrent).ToList();
        decimal Sum(string type, byte? flow = null) =>
            current.Where(e => e.TransactionType == type && (flow is null || e.Flow == flow))
                .Sum(e => e.Amount);

        var paid = Sum(FinancialTransactionTypes.CustomerPayment, FinancialFlow.In)
                   + Sum(FinancialTransactionTypes.AdditionalCharge, FinancialFlow.In);
        var refunds = Sum(FinancialTransactionTypes.CustomerRefund, FinancialFlow.Out);
        var additional = Sum(FinancialTransactionTypes.AdditionalCharge, FinancialFlow.In);

        return new OrderFinancialSummaryDto
        {
            OrderId = order.OrderID,
            OrderNumber = order.OrderNumber,
            CurrencyCode = order.CurrencyCode,
            ReportToTax = order.ReportToTax,
            CustomerPaidTotal = paid - additional,
            AdditionalChargesTotal = additional,
            RefundsTotal = refunds,
            NetReceived = paid - refunds,
            ShippingTotal = Sum(FinancialTransactionTypes.OrderShipping),
            MarkupTotal = Sum(FinancialTransactionTypes.OrderMarkup),
            ProductRevenueTotal = Sum(FinancialTransactionTypes.OrderLineRevenue),
            ProductCostTotal = Sum(FinancialTransactionTypes.OrderLineCost),
            ProductProfitTotal = Sum(FinancialTransactionTypes.OrderLineProfit),
            DiscountTotal = Sum(FinancialTransactionTypes.OrderDiscount),
            TaxTotal = Sum(FinancialTransactionTypes.OrderTax),
            VendorSettlementTotal = Sum(FinancialTransactionTypes.VendorSettlement, FinancialFlow.Out)
                                    + Sum(FinancialTransactionTypes.VendorSettlement, FinancialFlow.Component),
            OrderGrandTotalSnapshot = order.GrandTotal,
            Lines = current.OrderBy(e => e.OccurredDate).ThenBy(e => e.OccurredTime).Select(MapDto).ToList(),
            History = all.Where(e => !e.IsCurrent).Select(MapDto).ToList(),
        };
    }

    public async Task<IReadOnlyList<FinancialLedgerEntryDto>> GetVendorVisibleAsync(
        int vendorId, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default)
    {
        var q = _context.FinancialLedgerEntries.AsNoTracking()
            .Include(e => e.Order)
            .Where(e => e.VendorID == vendorId && e.VendorVisible && e.IsCurrent);
        if (from is DateOnly f) q = q.Where(e => e.OccurredDate >= f);
        if (to is DateOnly t) q = q.Where(e => e.OccurredDate <= t);
        var items = await q
            .OrderByDescending(e => e.OccurredDate).ThenByDescending(e => e.OccurredTime)
            .Take(500)
            .ToListAsync(ct);
        return items.Select(MapDto).ToList();
    }

    public async Task<FinancialLedgerEntry> PostAsync(PostFinancialEntryRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.TransactionType))
            throw new ArgumentException("TransactionType is required.");
        if (request.Amount < 0)
            throw new ArgumentException("Amount must be >= 0.");

        var occurredLocal = request.OccurredAtLocal ?? DateTime.Now;
        var occurredUtc = DateTime.SpecifyKind(occurredLocal, DateTimeKind.Local).ToUniversalTime();
        var amountUsd = request.AmountUsd ?? request.Amount;

        var entry = new FinancialLedgerEntry
        {
            WebsiteID = request.WebsiteId,
            TransactionType = request.TransactionType.Trim(),
            Flow = request.Flow,
            Amount = request.Amount,
            AmountUsd = amountUsd,
            CurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode) ? "USD" : request.CurrencyCode.Trim().ToUpperInvariant(),
            OccurredDate = DateOnly.FromDateTime(occurredLocal),
            OccurredTime = TimeOnly.FromDateTime(occurredLocal),
            OccurredAtUtc = occurredUtc,
            Title = string.IsNullOrWhiteSpace(request.Title) ? request.TransactionType : request.Title.Trim(),
            Description = request.Description,
            ReportToTax = request.ReportToTax,
            VendorVisible = request.VendorVisible,
            VendorID = request.VendorId,
            OrderID = request.OrderId,
            OrderItemID = request.OrderItemId,
            PaymentID = request.PaymentId,
            SettlementID = request.SettlementId,
            WebsiteClientID = request.WebsiteClientId,
            EventGroupId = request.EventGroupId ?? Guid.NewGuid(),
            Version = 1,
            IsCurrent = true,
            CreatedByMemberID = request.MemberId,
            CreatedAt = DateTime.UtcNow,
            MetaJson = request.MetaJson,
        };
        _context.FinancialLedgerEntries.Add(entry);
        await _context.SaveChangesAsync(ct);
        if (!request.SkipGlProjection)
        {
            try { await _gl.ProjectLedgerEntryAsync(entry.FinancialLedgerEntryID, ct); }
            catch { /* never break commercial flows */ }
        }
        return entry;
    }

    public async Task PostOrderPaidBreakdownAsync(int orderId, int? paymentId, int? memberId, CancellationToken ct = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
        if (order is null) return;

        // Drop previous current analytical/payment components for this order (keep history via supersede pattern:
        // mark non-current instead of delete).
        var existing = await _context.FinancialLedgerEntries
            .Where(e => e.OrderID == orderId && e.IsCurrent
                        && (e.TransactionType == FinancialTransactionTypes.CustomerPayment
                            || e.TransactionType == FinancialTransactionTypes.OrderShipping
                            || e.TransactionType == FinancialTransactionTypes.OrderTax
                            || e.TransactionType == FinancialTransactionTypes.OrderDiscount
                            || e.TransactionType == FinancialTransactionTypes.OrderMarkup
                            || e.TransactionType == FinancialTransactionTypes.OrderLineRevenue
                            || e.TransactionType == FinancialTransactionTypes.OrderLineCost
                            || e.TransactionType == FinancialTransactionTypes.OrderLineProfit))
            .ToListAsync(ct);

        // If we already have a matching payment posting for this payment id, skip re-post of payment only.
        var groupId = Guid.NewGuid();
        var nowLocal = DateTime.Now;
        if (order.PaidAt is DateTime paidAt)
            nowLocal = paidAt.Kind == DateTimeKind.Utc ? paidAt.ToLocalTime() : paidAt;

        foreach (var old in existing)
        {
            old.IsCurrent = false;
            old.ChangeNote = string.IsNullOrWhiteSpace(old.ChangeNote)
                ? "Superseded by order paid re-breakdown"
                : old.ChangeNote;
        }

        var reportTax = order.ReportToTax;
        var rate = order.ExchangeRateToUsd <= 0 ? 1m : order.ExchangeRateToUsd;

        decimal ToUsd(decimal local) =>
            order.GrandTotal > 0 && order.GrandTotalUsd > 0
                ? Math.Round(local * (order.GrandTotalUsd / order.GrandTotal), 4)
                : Math.Round(local * rate, 4);

        // Customer payment (cash in)
        var payAmount = order.GrandTotal;
        var payUsd = order.GrandTotalUsd;
        if (paymentId is int pid)
        {
            var payment = await _context.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.PaymentID == pid, ct);
            if (payment is not null)
            {
                payAmount = payment.Amount;
                payUsd = payment.AmountUsd;
            }
        }

        AddEntry(order, groupId, FinancialTransactionTypes.CustomerPayment, FinancialFlow.In,
            payAmount, payUsd,
            $"Payment for order {order.OrderNumber}",
            "Customer receipt", reportTax, vendorVisible: false, paymentId, null, null, memberId, nowLocal);

        // Components (memo)
        if (order.ShippingTotal > 0)
            AddEntry(order, groupId, FinancialTransactionTypes.OrderShipping, FinancialFlow.Component,
                order.ShippingTotal, ToUsd(order.ShippingTotal),
                "Shipping", null, reportTax, false, paymentId, null, null, memberId, nowLocal);

        if (order.TaxTotal > 0)
            AddEntry(order, groupId, FinancialTransactionTypes.OrderTax, FinancialFlow.Component,
                order.TaxTotal, ToUsd(order.TaxTotal),
                "Tax", null, reportTax, false, paymentId, null, null, memberId, nowLocal);

        if (order.DiscountTotal > 0)
            AddEntry(order, groupId, FinancialTransactionTypes.OrderDiscount, FinancialFlow.Component,
                order.DiscountTotal, ToUsd(order.DiscountTotal),
                "Discount", null, reportTax, false, paymentId, null, null, memberId, nowLocal);

        if (order.MarkupTotal > 0)
            AddEntry(order, groupId, FinancialTransactionTypes.OrderMarkup, FinancialFlow.Component,
                order.MarkupTotal, ToUsd(order.MarkupTotal),
                "Instant markup (روکشی)", null, reportTax, false, paymentId, null, null, memberId, nowLocal);

        foreach (var item in order.OrderItems)
        {
            var qty = item.Quantity <= 0 ? 1 : item.Quantity;
            var lineCatalog = item.CatalogUnitPrice > 0
                ? item.CatalogUnitPrice * qty
                : Math.Max(0, item.TotalPrice - item.UnitMarkup * qty);
            var lineMarkup = item.UnitMarkup * qty;
            // Cost: prefer USD dual converted proportionally to line currency
            var costUnitLocal = ResolveCostLocal(item);
            var lineCost = costUnitLocal * qty;
            var lineProfit = Math.Max(0, lineCatalog - lineCost);

            if (lineCatalog > 0)
                AddEntry(order, groupId, FinancialTransactionTypes.OrderLineRevenue, FinancialFlow.Component,
                    lineCatalog, ToUsd(lineCatalog),
                    $"Product revenue: {item.TitleSnapshot}",
                    $"SKU {item.SkuSnapshot} × {qty}", reportTax, false, paymentId, item.OrderItemID, item.VendorID, memberId, nowLocal);

            if (lineCost > 0)
                AddEntry(order, groupId, FinancialTransactionTypes.OrderLineCost, FinancialFlow.Component,
                    lineCost, item.UnitCostUsd > 0 ? item.UnitCostUsd * qty : ToUsd(lineCost),
                    $"Product cost: {item.TitleSnapshot}",
                    null, reportTax, vendorVisible: false, paymentId, item.OrderItemID, item.VendorID, memberId, nowLocal);

            if (lineProfit > 0)
                AddEntry(order, groupId, FinancialTransactionTypes.OrderLineProfit, FinancialFlow.Component,
                    lineProfit, ToUsd(lineProfit),
                    $"Product profit: {item.TitleSnapshot}",
                    null, reportTax, false, paymentId, item.OrderItemID, item.VendorID, memberId, nowLocal);

            // Markup already at order level; if per-line markup and order markup is 0, post per line
            if (order.MarkupTotal <= 0 && lineMarkup > 0)
                AddEntry(order, groupId, FinancialTransactionTypes.OrderMarkup, FinancialFlow.Component,
                    lineMarkup, ToUsd(lineMarkup),
                    $"Markup: {item.TitleSnapshot}",
                    null, reportTax, false, paymentId, item.OrderItemID, item.VendorID, memberId, nowLocal);
        }

        await _context.SaveChangesAsync(ct);
        try { await _gl.ProjectEventGroupAsync(order.WebsiteID, groupId, ct); }
        catch { /* never break commercial flows */ }
    }

    public async Task PostCustomerRefundAsync(int orderId, int paymentId, decimal amount, string? note, int? memberId, CancellationToken ct = default)
    {
        if (amount <= 0) return;
        var order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
        if (order is null) return;
        var payment = await _context.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.PaymentID == paymentId, ct);
        var usd = payment is not null && payment.Amount > 0
            ? Math.Round(amount * (payment.AmountUsd / payment.Amount), 4)
            : amount;

        await PostAsync(new PostFinancialEntryRequest
        {
            WebsiteId = order.WebsiteID,
            TransactionType = FinancialTransactionTypes.CustomerRefund,
            Flow = FinancialFlow.Out,
            Amount = amount,
            AmountUsd = usd,
            CurrencyCode = order.CurrencyCode,
            Title = $"Refund for order {order.OrderNumber}",
            Description = note,
            ReportToTax = order.ReportToTax,
            VendorVisible = false,
            OrderId = orderId,
            PaymentId = paymentId,
            WebsiteClientId = order.WebsiteClientID,
            MemberId = memberId,
        }, ct);
    }

    public async Task PostInventoryCogsForOrderAsync(int orderId, int? stockDocumentId, int? memberId, CancellationToken ct = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
        if (order is null) return;

        var sourceKey = stockDocumentId is int docId
            ? $"STOCK-COGS:{docId}"
            : $"STOCK-COGS:ORD:{orderId}";
        if (await _context.FinancialLedgerEntries.AnyAsync(
                e => e.WebsiteID == order.WebsiteID && e.IsCurrent
                     && e.TransactionType == FinancialTransactionTypes.InventoryCogs
                     && e.MetaJson != null && e.MetaJson.Contains(sourceKey), ct))
            return;

        decimal totalLocal = 0, totalUsd = 0;
        foreach (var item in order.OrderItems)
        {
            var qty = item.Quantity <= 0 ? 1 : item.Quantity;
            var costUnit = ResolveCostLocal(item);
            if (costUnit <= 0) continue;
            totalLocal += costUnit * qty;
            totalUsd += (item.UnitCostUsd > 0 ? item.UnitCostUsd : costUnit) * qty;
        }
        if (totalLocal <= 0) return;

        var groupId = Guid.NewGuid();
        await PostAsync(new PostFinancialEntryRequest
        {
            WebsiteId = order.WebsiteID,
            TransactionType = FinancialTransactionTypes.InventoryCogs,
            Flow = FinancialFlow.Component,
            Amount = totalLocal,
            AmountUsd = Math.Round(totalUsd, 4),
            CurrencyCode = order.CurrencyCode,
            Title = $"COGS stock-out order {order.OrderNumber}",
            Description = stockDocumentId is int d
                ? $"Warehouse outbound #{d}"
                : "Non-WMS fulfill (inventory left on payment)",
            ReportToTax = order.ReportToTax,
            VendorVisible = false,
            OrderId = orderId,
            WebsiteClientId = order.WebsiteClientID,
            EventGroupId = groupId,
            MemberId = memberId,
            MetaJson = $"{{\"sourceKey\":\"{sourceKey}\",\"stockDocumentId\":{(stockDocumentId?.ToString() ?? "null")}}}",
        }, ct);
    }

    public async Task PostInventoryCogsReversalForReturnAsync(int orderId, int stockDocumentId, int? memberId, CancellationToken ct = default)
    {
        var order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
        if (order is null) return;

        var sourceKey = $"STOCK-COGS-REV:{stockDocumentId}";
        if (await _context.FinancialLedgerEntries.AnyAsync(
                e => e.WebsiteID == order.WebsiteID && e.IsCurrent
                     && e.TransactionType == FinancialTransactionTypes.InventoryCogsReversal
                     && e.MetaJson != null && e.MetaJson.Contains(sourceKey), ct))
            return;

        // Only reverse sellable return lines (defective scrap stays out of inventory asset).
        var lines = await _context.StockDocumentLines.AsNoTracking()
            .Where(l => l.StockDocumentID == stockDocumentId
                        && l.ReturnCondition != (byte)Domain.Enums.StockItemCondition.Defective)
            .ToListAsync(ct);
        if (lines.Count == 0) return;

        decimal totalLocal = 0, totalUsd = 0;
        foreach (var line in lines)
        {
            var qty = Math.Abs(line.Quantity);
            if (qty <= 0) continue;
            totalLocal += line.UnitCost * qty;
            totalUsd += (line.UnitCostUsd > 0 ? line.UnitCostUsd : line.UnitCost) * qty;
        }
        if (totalLocal <= 0)
        {
            // Fall back to original order cost proportion if return lines have zero unit cost.
            var items = await _context.OrderItems.AsNoTracking().Where(i => i.OrderID == orderId).ToListAsync(ct);
            foreach (var item in items)
            {
                var qty = item.Quantity <= 0 ? 1 : item.Quantity;
                var costUnit = ResolveCostLocal(item);
                if (costUnit <= 0) continue;
                totalLocal += costUnit * qty;
                totalUsd += (item.UnitCostUsd > 0 ? item.UnitCostUsd : costUnit) * qty;
            }
        }
        if (totalLocal <= 0) return;

        var groupId = Guid.NewGuid();
        await PostAsync(new PostFinancialEntryRequest
        {
            WebsiteId = order.WebsiteID,
            TransactionType = FinancialTransactionTypes.InventoryCogsReversal,
            Flow = FinancialFlow.Component,
            Amount = totalLocal,
            AmountUsd = Math.Round(totalUsd, 4),
            CurrencyCode = order.CurrencyCode,
            Title = $"COGS reverse return order {order.OrderNumber}",
            Description = $"Warehouse return #{stockDocumentId}",
            ReportToTax = order.ReportToTax,
            VendorVisible = false,
            OrderId = orderId,
            WebsiteClientId = order.WebsiteClientID,
            EventGroupId = groupId,
            MemberId = memberId,
            MetaJson = $"{{\"sourceKey\":\"{sourceKey}\",\"stockDocumentId\":{stockDocumentId}}}",
        }, ct);
    }

    public async Task PostVendorSettlementAsync(
        int websiteId, int? vendorId, int? settlementId, int? orderId, int? orderItemId,
        decimal amount, string currencyCode, decimal amountUsd, string title, bool reportToTax,
        int? memberId, CancellationToken ct = default)
    {
        if (amount <= 0) return;
        await PostAsync(new PostFinancialEntryRequest
        {
            WebsiteId = websiteId,
            TransactionType = FinancialTransactionTypes.VendorSettlement,
            Flow = FinancialFlow.Out,
            Amount = amount,
            AmountUsd = amountUsd,
            CurrencyCode = currencyCode,
            Title = title,
            ReportToTax = reportToTax,
            VendorVisible = true,
            VendorId = vendorId,
            OrderId = orderId,
            OrderItemId = orderItemId,
            SettlementId = settlementId,
            MemberId = memberId,
        }, ct);
    }

    public async Task<(bool Success, string? Error, FinancialLedgerEntry? Entry)> PostAdditionalChargeAsync(
        PostAdditionalChargeRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            return (false, "Amount must be positive.", null);

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == request.OrderId, ct);
        if (order is null) return (false, "Order not found.", null);

        var title = string.IsNullOrWhiteSpace(request.Title) ? "Additional charge" : request.Title.Trim();
        var rate = order.GrandTotal > 0 && order.GrandTotalUsd > 0
            ? order.GrandTotalUsd / order.GrandTotal
            : (order.ExchangeRateToUsd <= 0 ? 1m : order.ExchangeRateToUsd);
        var usd = Math.Round(request.Amount * rate, 4);

        var groupId = Guid.NewGuid();
        var entry = await PostAsync(new PostFinancialEntryRequest
        {
            WebsiteId = order.WebsiteID,
            TransactionType = FinancialTransactionTypes.AdditionalCharge,
            Flow = FinancialFlow.In,
            Amount = request.Amount,
            AmountUsd = usd,
            CurrencyCode = order.CurrencyCode,
            Title = title,
            Description = request.Description,
            ReportToTax = request.ReportToTax,
            VendorVisible = false,
            OrderId = order.OrderID,
            WebsiteClientId = order.WebsiteClientID,
            EventGroupId = groupId,
            MemberId = request.MemberId,
            MetaJson = $"{{\"kind\":\"additional_charge\"}}",
        }, ct);

        if (request.RecordPayment)
        {
            var payment = new Payment
            {
                WebsiteID = order.WebsiteID,
                OrderID = order.OrderID,
                WebsiteClientID = order.WebsiteClientID,
                Method = request.PaymentMethod ?? (byte)3, // Manual
                Amount = request.Amount,
                CurrencyCode = order.CurrencyCode,
                ExchangeRateToUsd = order.ExchangeRateToUsd,
                AmountUsd = usd,
                Status = 2, // Paid
                TrackingCode = request.PaymentReference,
                PaidAt = DateTime.UtcNow,
                CreatedByMemberID = request.MemberId,
                CreatedAt = DateTime.UtcNow,
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync(ct);
            entry.PaymentID = payment.PaymentID;
            await _context.SaveChangesAsync(ct);
        }

        return (true, null, entry);
    }

    public async Task<(bool Success, string? Error, FinancialLedgerEntry? Entry)> SupersedeAsync(
        long entryId, decimal newAmount, string? newTitle, string? newDescription,
        bool? reportToTax, string changeNote, int? memberId, CancellationToken ct = default)
    {
        if (newAmount < 0) return (false, "Amount must be >= 0.", null);
        var old = await _context.FinancialLedgerEntries.FirstOrDefaultAsync(e => e.FinancialLedgerEntryID == entryId, ct);
        if (old is null) return (false, "Entry not found.", null);
        if (!old.IsCurrent) return (false, "Only current entries can be edited.", null);

        var oldAmount = old.Amount;
        old.IsCurrent = false;
        old.ChangeNote = string.IsNullOrWhiteSpace(changeNote)
            ? $"Edited: {oldAmount:0.####} → {newAmount:0.####}"
            : changeNote;

        var ratio = old.Amount > 0 ? newAmount / old.Amount : 1m;
        var entry = new FinancialLedgerEntry
        {
            WebsiteID = old.WebsiteID,
            TransactionType = old.TransactionType,
            Flow = old.Flow,
            Amount = newAmount,
            AmountUsd = Math.Round(old.AmountUsd * ratio, 4),
            CurrencyCode = old.CurrencyCode,
            OccurredDate = old.OccurredDate,
            OccurredTime = old.OccurredTime,
            OccurredAtUtc = old.OccurredAtUtc,
            Title = string.IsNullOrWhiteSpace(newTitle) ? old.Title : newTitle.Trim(),
            Description = newDescription ?? old.Description,
            ReportToTax = reportToTax ?? old.ReportToTax,
            VendorVisible = old.VendorVisible,
            VendorID = old.VendorID,
            OrderID = old.OrderID,
            OrderItemID = old.OrderItemID,
            PaymentID = old.PaymentID,
            SettlementID = old.SettlementID,
            WebsiteClientID = old.WebsiteClientID,
            EventGroupId = old.EventGroupId,
            Version = old.Version + 1,
            SupersedesEntryID = old.FinancialLedgerEntryID,
            IsCurrent = true,
            ChangeNote = $"Was {oldAmount:0.####} {old.CurrencyCode}",
            CreatedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
            MetaJson = old.MetaJson,
        };
        _context.FinancialLedgerEntries.Add(entry);
        await _context.SaveChangesAsync(ct);
        return (true, null, entry);
    }

    // ── helpers ──────────────────────────────────────────────────────

    private void AddEntry(
        Order order, Guid groupId, string type, byte flow,
        decimal amount, decimal amountUsd, string title, string? description,
        bool reportToTax, bool vendorVisible, int? paymentId, int? orderItemId, int? vendorId,
        int? memberId, DateTime occurredLocal)
    {
        if (amount <= 0) return;
        var occurredUtc = DateTime.SpecifyKind(occurredLocal, DateTimeKind.Local).ToUniversalTime();
        _context.FinancialLedgerEntries.Add(new FinancialLedgerEntry
        {
            WebsiteID = order.WebsiteID,
            TransactionType = type,
            Flow = flow,
            Amount = amount,
            AmountUsd = amountUsd,
            CurrencyCode = order.CurrencyCode,
            OccurredDate = DateOnly.FromDateTime(occurredLocal),
            OccurredTime = TimeOnly.FromDateTime(occurredLocal),
            OccurredAtUtc = occurredUtc,
            Title = title,
            Description = description,
            ReportToTax = reportToTax,
            VendorVisible = vendorVisible,
            VendorID = vendorId,
            OrderID = order.OrderID,
            OrderItemID = orderItemId,
            PaymentID = paymentId,
            WebsiteClientID = order.WebsiteClientID,
            EventGroupId = groupId,
            Version = 1,
            IsCurrent = true,
            CreatedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
        });
    }

    private static decimal ResolveCostLocal(OrderItem item)
    {
        if (item.UnitCostUsd <= 0) return 0;
        if (item.UnitPriceUsd > 0 && item.UnitPrice > 0)
            return item.UnitCostUsd * (item.UnitPrice / item.UnitPriceUsd);
        if (item.CatalogUnitPrice > 0 && item.UnitPriceUsd > 0)
            return item.UnitCostUsd * (item.CatalogUnitPrice / Math.Max(item.UnitPriceUsd, 0.0001m));
        return item.UnitCostUsd;
    }

    private static IQueryable<FinancialLedgerEntry> ApplyFilter(
        IQueryable<FinancialLedgerEntry> q, FinancialLedgerFilter filter)
    {
        if (filter.WebsiteId is int wid) q = q.Where(e => e.WebsiteID == wid);
        if (filter.CurrentOnly) q = q.Where(e => e.IsCurrent);
        if (filter.VendorVisibleOnly) q = q.Where(e => e.VendorVisible);
        if (!string.IsNullOrWhiteSpace(filter.TransactionType))
            q = q.Where(e => e.TransactionType == filter.TransactionType);
        if (filter.Flow is byte flow) q = q.Where(e => e.Flow == flow);
        if (filter.OrderId is int oid) q = q.Where(e => e.OrderID == oid);
        if (filter.VendorId is int vid) q = q.Where(e => e.VendorID == vid);
        if (filter.WebsiteClientId is int cid) q = q.Where(e => e.WebsiteClientID == cid);
        if (filter.FromDate is DateOnly from) q = q.Where(e => e.OccurredDate >= from);
        if (filter.ToDate is DateOnly to) q = q.Where(e => e.OccurredDate <= to);
        if (filter.ReportToTax is bool tax) q = q.Where(e => e.ReportToTax == tax);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();
            q = q.Where(e => e.Title.Contains(s)
                             || (e.Description != null && e.Description.Contains(s))
                             || e.TransactionType.Contains(s));
        }
        return q;
    }

    private static FinancialLedgerEntryDto MapDto(FinancialLedgerEntry e) => new()
    {
        FinancialLedgerEntryID = e.FinancialLedgerEntryID,
        WebsiteID = e.WebsiteID,
        TransactionType = e.TransactionType,
        Flow = e.Flow,
        Amount = e.Amount,
        AmountUsd = e.AmountUsd,
        CurrencyCode = e.CurrencyCode,
        OccurredDate = e.OccurredDate,
        OccurredTime = e.OccurredTime,
        OccurredAtUtc = e.OccurredAtUtc,
        Title = e.Title,
        Description = e.Description,
        ReportToTax = e.ReportToTax,
        VendorVisible = e.VendorVisible,
        VendorID = e.VendorID,
        VendorName = e.Vendor?.Name,
        OrderID = e.OrderID,
        OrderNumber = e.Order?.OrderNumber,
        OrderItemID = e.OrderItemID,
        PaymentID = e.PaymentID,
        SettlementID = e.SettlementID,
        WebsiteClientID = e.WebsiteClientID,
        EventGroupId = e.EventGroupId,
        Version = e.Version,
        SupersedesEntryID = e.SupersedesEntryID,
        IsCurrent = e.IsCurrent,
        ChangeNote = e.ChangeNote,
        CreatedByMemberID = e.CreatedByMemberID,
        CreatedAt = e.CreatedAt,
        MetaJson = e.MetaJson,
    };
}
