using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.RecordAttachments;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class CustomerReturnService : ICustomerReturnService
{
    private readonly IDbContextFactory<AppDbContext> _db;
    private readonly IStockDocumentService _stockDocs;
    private readonly IRecordAttachmentService _attachments;

    public CustomerReturnService(
        IDbContextFactory<AppDbContext> db,
        IStockDocumentService stockDocs,
        IRecordAttachmentService attachments)
    {
        _db = db;
        _stockDocs = stockDocs;
        _attachments = attachments;
    }

    public Task<PagedResult<CustomerReturnDto>> GetPagedAsync(int websiteId, int? clientId, byte? status, GridQuery query, CancellationToken ct = default)
        => _db.UseAsync(async (context, token) =>
        {
            var q = context.CustomerReturnRequests.AsNoTracking()
                .Include(r => r.Order)
                .Include(r => r.WebsiteClient)
                .Where(r => r.WebsiteID == websiteId);
            if (clientId is int cid) q = q.Where(r => r.WebsiteClientID == cid);
            if (status is byte s) q = q.Where(r => r.Status == s);

            if (query.GetSearch(nameof(CustomerReturnRequest.TrackingCode)) is string track)
                q = q.Where(r => r.TrackingCode != null && r.TrackingCode.Contains(track));
            if (query.GetSearch("OrderNumber") is string on)
                q = q.Where(r => r.Order.OrderNumber.Contains(on));
            if (query.GetSearch(nameof(CustomerReturnRequest.Status)) is string st && byte.TryParse(st, out var sf))
                q = q.Where(r => r.Status == sf);

            var total = await q.CountAsync(token);
            var items = await q
                .ApplyOrderBy(query.OrderBy, nameof(CustomerReturnRequest.CreatedAt), fallbackDescending: true)
                .Skip(query.Skip).Take(query.Take)
                .ToListAsync(token);
            return new PagedResult<CustomerReturnDto>
            {
                Items = items.Select(r => MapHeader(r)).ToList(),
                TotalCount = total,
            };
        }, ct);

    public async Task<CustomerReturnDto?> GetByIdAsync(int returnRequestId, int? clientId = null, CancellationToken ct = default)
    {
        await using var context = await _db.CreateDbContextAsync(ct);
        var q = context.CustomerReturnRequests.AsNoTracking()
            .Include(r => r.Order).ThenInclude(o => o.OrderItems)
            .Include(r => r.WebsiteClient)
            .Include(r => r.Lines).ThenInclude(l => l.OrderItem)
            .Include(r => r.Histories)
            .Where(r => r.CustomerReturnRequestID == returnRequestId);
        if (clientId is int cid) q = q.Where(r => r.WebsiteClientID == cid);
        var row = await q.FirstOrDefaultAsync(ct);
        if (row is null) return null;

        var remaining = await RemainingByOrderItemAsync(context, row.OrderID, row.CustomerReturnRequestID, ct);
        var photos = await _attachments.ListAsync(RecordEntityTypes.CustomerReturnRequest, row.CustomerReturnRequestID, ct);
        return MapDetail(row, remaining, photos);
    }

    public async Task<ReturnEligibilityDto> GetEligibilityAsync(int orderId, int clientId, CancellationToken ct = default)
    {
        await using var context = await _db.CreateDbContextAsync(ct);
        var order = await context.Orders.AsNoTracking()
            .Include(o => o.OrderItems)
            .Include(o => o.Website)
            .FirstOrDefaultAsync(o => o.OrderID == orderId && o.WebsiteClientID == clientId, ct);
        if (order is null)
            return new ReturnEligibilityDto { Eligible = false, BlockReason = "Order not found." };

        var website = order.Website ?? await context.Websites.AsNoTracking().FirstAsync(w => w.WebsiteID == order.WebsiteID, ct);
        var remaining = await RemainingByOrderItemAsync(context, order.OrderID, excludeRequestId: null, ct);
        var (ok, err, start, end) = EvaluateWindow(order, website);
        var lines = order.OrderItems.Select(i =>
        {
            remaining.TryGetValue(i.OrderItemID, out var rem);
            var ordered = i.Quantity;
            return new ReturnableOrderLineDto
            {
                OrderItemID = i.OrderItemID,
                ProductVariantID = i.ProductVariantID,
                Title = i.TitleSnapshot,
                Sku = i.SkuSnapshot,
                OrderedQty = ordered,
                RemainingQty = rem,
                AlreadyRequestedQty = Math.Max(0, ordered - rem),
                UnitPricePaid = UnitPaid(i),
            };
        }).ToList();

        var anyLeft = lines.Any(l => l.RemainingQty > 0);
        if (ok && !anyLeft)
        {
            ok = false;
            err = "No remaining quantity to return.";
        }

        return new ReturnEligibilityDto
        {
            OrderID = order.OrderID,
            OrderNumber = order.OrderNumber,
            Eligible = ok,
            BlockReason = err,
            WindowStartUtc = start,
            WindowEndUtc = end,
            ReturnWindowDays = website.ReturnWindowDays,
            ReturnWindowFrom = website.ReturnWindowFrom,
            CurrencyCode = order.CurrencyCode,
            Lines = lines,
        };
    }

    public async Task<(bool Success, string? Error, CustomerReturnDto? Request)> CreateAsync(
        int websiteId, int clientId, int orderId,
        CustomerReturnReason reason, string? reasonNote, string? description,
        string? shipMethod, IReadOnlyList<CustomerReturnLineInput> lines,
        IReadOnlyList<int>? photoFileIds, CancellationToken ct = default)
    {
        if (lines is null || lines.Count == 0)
            return (false, "Select at least one item to return.", null);

        await using var context = await _db.CreateDbContextAsync(ct);
        var order = await context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Website)
            .FirstOrDefaultAsync(o => o.OrderID == orderId && o.WebsiteClientID == clientId && o.WebsiteID == websiteId, ct);
        if (order is null) return (false, "Order not found.", null);

        var website = order.Website ?? await context.Websites.FirstAsync(w => w.WebsiteID == websiteId, ct);
        var (ok, err, _, _) = EvaluateWindow(order, website);
        if (!ok) return (false, err, null);

        var remaining = await RemainingByOrderItemAsync(context, order.OrderID, null, ct);
        var items = new List<CustomerReturnRequestLine>();
        decimal requestedTotal = 0;
        foreach (var input in lines.Where(l => l.Quantity > 0))
        {
            var oi = order.OrderItems.FirstOrDefault(i => i.OrderItemID == input.OrderItemID);
            if (oi is null) return (false, $"Order item {input.OrderItemID} is not on this order.", null);
            remaining.TryGetValue(oi.OrderItemID, out var rem);
            if (input.Quantity > rem)
                return (false, $"Only {rem} remaining for {oi.TitleSnapshot}.", null);
            var paid = UnitPaid(oi);
            var req = input.UnitRefundRequested > 0 ? input.UnitRefundRequested : paid;
            items.Add(new CustomerReturnRequestLine
            {
                OrderItemID = oi.OrderItemID,
                ProductVariantID = oi.ProductVariantID,
                Quantity = input.Quantity,
                UnitPricePaid = paid,
                UnitRefundRequested = req,
                UnitRefundApproved = 0,
            });
            requestedTotal += req * input.Quantity;
        }
        if (items.Count == 0) return (false, "Select at least one item to return.", null);

        var reqRow = new CustomerReturnRequest
        {
            WebsiteID = websiteId,
            OrderID = order.OrderID,
            WebsiteClientID = clientId,
            Status = (byte)CustomerReturnStatus.PreRequest,
            Reason = (byte)reason,
            ReasonNote = Truncate(reasonNote, 200),
            Description = Truncate(description, 2000),
            ShipMethod = Truncate(shipMethod, 100),
            CurrencyCode = order.CurrencyCode,
            RequestedRefundTotal = requestedTotal,
            CreatedAt = DateTime.UtcNow,
        };
        foreach (var line in items) reqRow.Lines.Add(line);
        reqRow.Histories.Add(History(0, CustomerReturnStatus.PreRequest, "Pre-request submitted", null, clientId));
        context.CustomerReturnRequests.Add(reqRow);
        await context.SaveChangesAsync(ct);

        if (photoFileIds is { Count: > 0 })
        {
            foreach (var fileId in photoFileIds.Distinct().Where(id => id > 0))
                await _attachments.AttachAsync(websiteId, RecordEntityTypes.CustomerReturnRequest, reqRow.CustomerReturnRequestID, fileId, "Return photo", null, null, ct);
        }

        var dto = await GetByIdAsync(reqRow.CustomerReturnRequestID, clientId, ct);
        return (true, null, dto);
    }

    public async Task<(bool Success, string? Error)> AttachPhotoAsync(int returnRequestId, int clientId, int fileRecordId, CancellationToken ct = default)
    {
        await using var context = await _db.CreateDbContextAsync(ct);
        var row = await context.CustomerReturnRequests.FirstOrDefaultAsync(
            r => r.CustomerReturnRequestID == returnRequestId && r.WebsiteClientID == clientId, ct);
        if (row is null) return (false, "Return not found.");
        if (row.Status is (byte)CustomerReturnStatus.Rejected or (byte)CustomerReturnStatus.Cancelled or (byte)CustomerReturnStatus.Completed)
            return (false, "Cannot add photos to this return.");
        var (ok, err, _) = await _attachments.AttachAsync(
            row.WebsiteID, RecordEntityTypes.CustomerReturnRequest, row.CustomerReturnRequestID, fileRecordId, "Return photo", null, null, ct);
        return ok ? (true, null) : (false, err);
    }

    public async Task<(bool Success, string? Error)> ApproveAsync(
        int returnRequestId, ReturnShippingPayer shippingPayer, string? reviewNote,
        IReadOnlyList<CustomerReturnLineApproval> lineApprovals, int memberId, CancellationToken ct = default)
    {
        if (shippingPayer is ReturnShippingPayer.Unset)
            return (false, "Choose who pays return shipping (site or customer) before approving.");

        await using var context = await _db.CreateDbContextAsync(ct);
        var row = await context.CustomerReturnRequests
            .Include(r => r.Lines)
            .Include(r => r.Order).ThenInclude(o => o.OrderItems)
            .FirstOrDefaultAsync(r => r.CustomerReturnRequestID == returnRequestId, ct);
        if (row is null) return (false, "Return not found.");
        if (row.Status != (byte)CustomerReturnStatus.PreRequest)
            return (false, "Only a pre-request can be approved.");

        decimal approvedTotal = 0;
        foreach (var line in row.Lines)
        {
            var approval = lineApprovals?.FirstOrDefault(a => a.CustomerReturnRequestLineID == line.CustomerReturnRequestLineID);
            var amount = approval is not null && approval.UnitRefundApproved >= 0
                ? approval.UnitRefundApproved
                : line.UnitRefundRequested;
            line.UnitRefundApproved = amount;
            approvedTotal += amount * line.Quantity;
        }

        var wmsLines = row.Lines
            .Where(l => l.ProductVariantID is int)
            .Select(l => new StockDocumentLineRequest
            {
                ProductVariantID = l.ProductVariantID!.Value,
                Quantity = l.Quantity,
                UnitCost = l.UnitPricePaid,
                Note = l.OrderItem?.SkuSnapshot,
            }).ToList();

        if (wmsLines.Count > 0)
        {
            var (ok, err, doc) = await _stockDocs.CreateCustomerReturnAsync(
                row.OrderID, null, wmsLines, $"RMA #{row.CustomerReturnRequestID}", memberId, ct);
            if (!ok) return (false, err ?? "Could not create warehouse return.");
            row.StockDocumentID = doc?.StockDocumentID;
        }

        row.ShippingPayer = (byte)shippingPayer;
        row.ApprovedRefundTotal = approvedTotal;
        row.ReviewNote = Truncate(reviewNote, 1000);
        row.ReviewedByMemberID = memberId;
        row.ReviewedAt = DateTime.UtcNow;
        row.UpdatedAt = DateTime.UtcNow;
        var from = row.Status;
        row.Status = (byte)CustomerReturnStatus.ApprovedAwaitingShipment;
        row.Histories.Add(History(from, CustomerReturnStatus.ApprovedAwaitingShipment,
            $"Approved. Shipping paid by {(shippingPayer == ReturnShippingPayer.Site ? "site" : "customer")}.", memberId, null));
        await context.SaveChangesAsync(ct);
        return (true, null);
    }

    public Task<(bool Success, string? Error)> RejectAsync(int returnRequestId, string? reviewNote, int memberId, CancellationToken ct = default)
        => TransitionAsync(returnRequestId, CustomerReturnStatus.PreRequest, CustomerReturnStatus.Rejected, memberId, null,
            reviewNote ?? "Rejected", ct, extra: row =>
            {
                row.ReviewNote = Truncate(reviewNote, 1000);
                row.ReviewedByMemberID = memberId;
                row.ReviewedAt = DateTime.UtcNow;
            });

    public async Task<(bool Success, string? Error)> SubmitShipmentAsync(
        int returnRequestId, int clientId, string? shipMethod, string trackingCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(trackingCode))
            return (false, "Tracking code is required after you ship the parcel.");
        await using var context = await _db.CreateDbContextAsync(ct);
        var row = await context.CustomerReturnRequests.FirstOrDefaultAsync(
            r => r.CustomerReturnRequestID == returnRequestId && r.WebsiteClientID == clientId, ct);
        if (row is null) return (false, "Return not found.");
        if (row.Status != (byte)CustomerReturnStatus.ApprovedAwaitingShipment)
            return (false, "Ship only after the pre-request is approved.");
        var from = row.Status;
        row.TrackingCode = Truncate(trackingCode.Trim(), 100);
        if (!string.IsNullOrWhiteSpace(shipMethod)) row.ShipMethod = Truncate(shipMethod, 100);
        row.ShippedAt = DateTime.UtcNow;
        row.Status = (byte)CustomerReturnStatus.Shipped;
        row.UpdatedAt = DateTime.UtcNow;
        row.Histories.Add(History(from, CustomerReturnStatus.Shipped, $"Shipped. Tracking {row.TrackingCode}", null, clientId));
        await context.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateTrackingAsync(
        int returnRequestId, int clientId, string trackingCode, string? shipMethod, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(trackingCode))
            return (false, "Tracking code is required.");
        await using var context = await _db.CreateDbContextAsync(ct);
        var row = await context.CustomerReturnRequests.FirstOrDefaultAsync(
            r => r.CustomerReturnRequestID == returnRequestId && r.WebsiteClientID == clientId, ct);
        if (row is null) return (false, "Return not found.");
        if (row.Status is not ((byte)CustomerReturnStatus.ApprovedAwaitingShipment or (byte)CustomerReturnStatus.Shipped))
            return (false, "Tracking can only be updated before the parcel is received.");
        row.TrackingCode = Truncate(trackingCode.Trim(), 100);
        if (!string.IsNullOrWhiteSpace(shipMethod)) row.ShipMethod = Truncate(shipMethod, 100);
        row.UpdatedAt = DateTime.UtcNow;
        row.Histories.Add(History(row.Status, (CustomerReturnStatus)row.Status, $"Tracking updated to {row.TrackingCode}", null, clientId));
        await context.SaveChangesAsync(ct);
        return (true, null);
    }

    public Task<(bool Success, string? Error)> MarkReceivedAsync(int returnRequestId, int memberId, CancellationToken ct = default)
        => TransitionAsync(returnRequestId, CustomerReturnStatus.Shipped, CustomerReturnStatus.Received, memberId, null, "Received at warehouse", ct);

    public Task<(bool Success, string? Error)> MarkCompletedAsync(int returnRequestId, int memberId, CancellationToken ct = default)
        => TransitionAsync(returnRequestId, null, CustomerReturnStatus.Completed, memberId, null, "Completed", ct,
            allowFrom: [(byte)CustomerReturnStatus.Received, (byte)CustomerReturnStatus.Shipped, (byte)CustomerReturnStatus.ApprovedAwaitingShipment]);

    public async Task<(bool Success, string? Error)> CancelAsync(int returnRequestId, int? clientId, int? memberId, string? note, CancellationToken ct = default)
    {
        await using var context = await _db.CreateDbContextAsync(ct);
        var row = await context.CustomerReturnRequests.FirstOrDefaultAsync(r => r.CustomerReturnRequestID == returnRequestId, ct);
        if (row is null) return (false, "Return not found.");
        if (clientId is int cid && row.WebsiteClientID != cid) return (false, "Return not found.");
        if (row.Status is (byte)CustomerReturnStatus.Completed or (byte)CustomerReturnStatus.Received)
            return (false, "Cannot cancel after the parcel is received.");
        var from = row.Status;
        row.Status = (byte)CustomerReturnStatus.Cancelled;
        row.UpdatedAt = DateTime.UtcNow;
        row.Histories.Add(History(from, CustomerReturnStatus.Cancelled, note ?? "Cancelled", memberId, clientId));
        await context.SaveChangesAsync(ct);
        return (true, null);
    }

    private async Task<(bool Success, string? Error)> TransitionAsync(
        int id, CustomerReturnStatus? fromRequired, CustomerReturnStatus to, int memberId, int? clientId, string note, CancellationToken ct,
        byte[]? allowFrom = null, Action<CustomerReturnRequest>? extra = null)
    {
        await using var context = await _db.CreateDbContextAsync(ct);
        var row = await context.CustomerReturnRequests.FirstOrDefaultAsync(r => r.CustomerReturnRequestID == id, ct);
        if (row is null) return (false, "Return not found.");
        if (fromRequired is CustomerReturnStatus req && row.Status != (byte)req)
            return (false, "Invalid status for this action.");
        if (allowFrom is { Length: > 0 } && !allowFrom.Contains(row.Status))
            return (false, "Invalid status for this action.");
        extra?.Invoke(row);
        var from = row.Status;
        row.Status = (byte)to;
        row.UpdatedAt = DateTime.UtcNow;
        row.Histories.Add(History(from, to, note, memberId, clientId));
        await context.SaveChangesAsync(ct);
        return (true, null);
    }

    private static async Task<Dictionary<int, int>> RemainingByOrderItemAsync(
        AppDbContext context, int orderId, int? excludeRequestId, CancellationToken ct)
    {
        var ordered = await context.OrderItems.AsNoTracking()
            .Where(i => i.OrderID == orderId)
            .Select(i => new { i.OrderItemID, i.Quantity })
            .ToListAsync(ct);
        var used = await context.CustomerReturnRequestLines.AsNoTracking()
            .Where(l => l.ReturnRequest.OrderID == orderId
                && l.ReturnRequest.Status != (byte)CustomerReturnStatus.Rejected
                && l.ReturnRequest.Status != (byte)CustomerReturnStatus.Cancelled
                && (excludeRequestId == null || l.CustomerReturnRequestID != excludeRequestId))
            .GroupBy(l => l.OrderItemID)
            .Select(g => new { OrderItemID = g.Key, Qty = g.Sum(x => x.Quantity) })
            .ToListAsync(ct);
        var usedMap = used.ToDictionary(x => x.OrderItemID, x => x.Qty);
        return ordered.ToDictionary(o => o.OrderItemID, o => Math.Max(0, o.Quantity - usedMap.GetValueOrDefault(o.OrderItemID)));
    }

    private static (bool Ok, string? Error, DateTime? Start, DateTime? End) EvaluateWindow(Order order, Website website)
    {
        if (!website.ReturnsEnabled)
            return (false, "Returns are disabled for this site.", null, null);
        if (order.Status is (byte)OrderStatus.Cancelled)
            return (false, "Cancelled orders cannot be returned.", null, null);
        if (order.PaidAt is null)
            return (false, "Only paid orders can be returned.", null, null);

        DateTime? start = null;
        if (website.ReturnWindowFrom == (byte)ReturnWindowFrom.Delivered)
        {
            if (order.ShippingStatus != (byte)OrderShippingStatus.Delivered)
                return (false, "Return window starts after the order is marked delivered.", null, null);
            start = order.ShippedAt ?? order.PaidAt;
        }
        else
        {
            if (order.ShippedAt is null && order.ShippingStatus < (byte)OrderShippingStatus.Shipped)
                return (false, "Return window starts after the order is shipped.", null, null);
            start = order.ShippedAt ?? order.PaidAt;
        }

        var days = website.ReturnWindowDays < 0 ? 0 : website.ReturnWindowDays;
        var end = start!.Value.AddDays(days);
        if (DateTime.UtcNow > end)
            return (false, $"Return window ({days} days) has expired.", start, end);
        return (true, null, start, end);
    }

    private static decimal UnitPaid(OrderItem i) =>
        i.Quantity > 0 ? decimal.Round(i.TotalPrice / i.Quantity, 4) : i.UnitPrice;

    private static CustomerReturnRequestHistory History(byte from, CustomerReturnStatus to, string? note, int? memberId, int? clientId) =>
        new()
        {
            FromStatus = from,
            ToStatus = (byte)to,
            Note = Truncate(note, 500),
            CreatedByMemberID = memberId,
            CreatedByClientID = clientId,
            CreatedAt = DateTime.UtcNow,
        };

    private static string? Truncate(string? s, int max) =>
        string.IsNullOrWhiteSpace(s) ? null : (s.Trim().Length <= max ? s.Trim() : s.Trim()[..max]);

    private static CustomerReturnDto MapHeader(CustomerReturnRequest r) => new()
    {
        CustomerReturnRequestID = r.CustomerReturnRequestID,
        WebsiteID = r.WebsiteID,
        OrderID = r.OrderID,
        OrderNumber = r.Order?.OrderNumber ?? "",
        WebsiteClientID = r.WebsiteClientID,
        ClientName = r.WebsiteClient is null ? null : $"{r.WebsiteClient.Givenname} {r.WebsiteClient.Surname}".Trim(),
        StockDocumentID = r.StockDocumentID,
        Status = r.Status,
        Reason = r.Reason,
        ReasonNote = r.ReasonNote,
        Description = r.Description,
        ShipMethod = r.ShipMethod,
        TrackingCode = r.TrackingCode,
        ShippingPayer = r.ShippingPayer,
        CurrencyCode = r.CurrencyCode,
        RequestedRefundTotal = r.RequestedRefundTotal,
        ApprovedRefundTotal = r.ApprovedRefundTotal,
        ReviewNote = r.ReviewNote,
        ReviewedAt = r.ReviewedAt,
        ShippedAt = r.ShippedAt,
        CreatedAt = r.CreatedAt,
    };

    private static CustomerReturnDto MapDetail(
        CustomerReturnRequest r, IReadOnlyDictionary<int, int> remaining, IReadOnlyList<RecordAttachmentDto> photos)
    {
        var dto = MapHeader(r);
        dto.Lines = r.Lines.Select(l => new CustomerReturnLineDto
        {
            CustomerReturnRequestLineID = l.CustomerReturnRequestLineID,
            OrderItemID = l.OrderItemID,
            ProductVariantID = l.ProductVariantID,
            Title = l.OrderItem?.TitleSnapshot ?? "",
            Sku = l.OrderItem?.SkuSnapshot ?? "",
            Quantity = l.Quantity,
            OrderedQty = l.OrderItem?.Quantity ?? l.Quantity,
            RemainingQty = remaining.GetValueOrDefault(l.OrderItemID),
            UnitPricePaid = l.UnitPricePaid,
            UnitRefundRequested = l.UnitRefundRequested,
            UnitRefundApproved = l.UnitRefundApproved,
        }).ToList();
        dto.Histories = r.Histories.OrderBy(h => h.CreatedAt).Select(h => new CustomerReturnHistoryDto
        {
            FromStatus = h.FromStatus,
            ToStatus = h.ToStatus,
            Note = h.Note,
            CreatedAt = h.CreatedAt,
        }).ToList();
        dto.Photos = photos;
        return dto;
    }
}
