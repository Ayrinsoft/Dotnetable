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
    private readonly IFinancialLedgerService _ledger;

    public CustomerReturnService(
        IDbContextFactory<AppDbContext> db,
        IStockDocumentService stockDocs,
        IRecordAttachmentService attachments,
        IFinancialLedgerService ledger)
    {
        _db = db;
        _stockDocs = stockDocs;
        _attachments = attachments;
        _ledger = ledger;
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
            .Include(r => r.ReceivedWarehouse)
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
        var (ok, err, start, end, expired) = EvaluateWindow(order, website);
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
        string? block = err;
        if (ok && !anyLeft)
        {
            ok = false;
            block = "No remaining quantity to return.";
        }

        return new ReturnEligibilityDto
        {
            OrderID = order.OrderID,
            OrderNumber = order.OrderNumber,
            Eligible = ok && anyLeft && !expired,
            BlockReason = !ok ? block : !anyLeft ? "No remaining quantity to return." : expired ? err : null,
            WindowExpired = expired,
            CanAcceptAfterWindow = ok && anyLeft && expired,
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
        IReadOnlyList<int>? photoFileIds,
        ReturnShippingPayer shippingPayer = ReturnShippingPayer.Unset,
        bool acceptExpiredWindow = false,
        CancellationToken ct = default)
    {
        if (lines is null || lines.Count == 0)
            return (false, "Select at least one item to return.", null);
        if (shippingPayer is ReturnShippingPayer.Unset)
            return (false, "Choose who pays return shipping.", null);

        await using var context = await _db.CreateDbContextAsync(ct);
        var order = await context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Website)
            .FirstOrDefaultAsync(o => o.OrderID == orderId && o.WebsiteClientID == clientId && o.WebsiteID == websiteId, ct);
        if (order is null) return (false, "Order not found.", null);

        var website = order.Website ?? await context.Websites.FirstAsync(w => w.WebsiteID == websiteId, ct);
        var (ok, err, _, _, expired) = EvaluateWindow(order, website);
        if (!ok) return (false, err, null);
        if (expired && !acceptExpiredWindow)
            return (false, "Return window has expired. Confirm you accept it anyway.", null);

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
                UnitCost = UnitCostLocal(oi),
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
            ShipMethod = Truncate(
                shippingPayer == ReturnShippingPayer.DropOffAtCenter && string.IsNullOrWhiteSpace(shipMethod)
                    ? "Drop-off at center"
                    : shipMethod, 100),
            ShippingPayer = (byte)shippingPayer,
            AcceptedAfterWindowExpired = expired && acceptExpiredWindow,
            CurrencyCode = order.CurrencyCode,
            RequestedRefundTotal = requestedTotal,
            CreatedAt = DateTime.UtcNow,
        };
        foreach (var line in items) reqRow.Lines.Add(line);
        var submitNote = expired && acceptExpiredWindow
            ? $"Pre-request submitted after return window (accepted). Shipping: {ReturnShippingShare.Label(shippingPayer)}."
            : $"Pre-request submitted. Shipping: {ReturnShippingShare.Label(shippingPayer)}.";
        reqRow.Histories.Add(History(0, CustomerReturnStatus.PreRequest, submitNote, null, clientId));
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
        IReadOnlyList<CustomerReturnLineApproval> lineApprovals, int memberId,
        decimal returnShippingCost = 0, int? receivedWarehouseId = null, CancellationToken ct = default)
    {
        if (shippingPayer is ReturnShippingPayer.Unset)
            return (false, "Choose who pays return shipping before approving.");
        if (shippingPayer is ReturnShippingPayer.DropOffAtCenter)
            returnShippingCost = 0;

        await using var context = await _db.CreateDbContextAsync(ct);
        var row = await context.CustomerReturnRequests
            .Include(r => r.Lines).ThenInclude(l => l.OrderItem)
            .Include(r => r.Order).ThenInclude(o => o.OrderItems)
            .FirstOrDefaultAsync(r => r.CustomerReturnRequestID == returnRequestId, ct);
        if (row is null) return (false, "Return not found.");
        if (row.Status != (byte)CustomerReturnStatus.PreRequest)
            return (false, "Only a pre-request can be approved.");

        if (receivedWarehouseId is int wid && wid > 0)
        {
            var whOk = await context.Warehouses.AnyAsync(w => w.WarehouseID == wid && w.WebsiteID == row.WebsiteID, ct);
            if (!whOk) return (false, "Warehouse not found on this site.");
            row.ReceivedWarehouseID = wid;
        }

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
                UnitCost = l.UnitCost > 0 ? l.UnitCost : l.UnitPricePaid,
                Note = l.OrderItem?.SkuSnapshot,
            }).ToList();

        if (wmsLines.Count > 0)
        {
            var (ok, err, doc) = await _stockDocs.CreateCustomerReturnAsync(
                row.OrderID, receivedWarehouseId, wmsLines, $"RMA #{row.CustomerReturnRequestID}", memberId, ct);
            if (!ok) return (false, err ?? "Could not create warehouse return.");
            row.StockDocumentID = doc?.StockDocumentID;
        }

        row.ShippingPayer = (byte)shippingPayer;
        row.ReturnShippingCost = returnShippingCost < 0 ? 0 : decimal.Round(returnShippingCost, 4);
        row.ApprovedRefundTotal = approvedTotal;
        row.ReviewNote = Truncate(reviewNote, 1000);
        row.ReviewedByMemberID = memberId;
        row.ReviewedAt = DateTime.UtcNow;
        row.UpdatedAt = DateTime.UtcNow;
        ApplyImpact(row);
        var from = row.Status;
        row.Status = (byte)CustomerReturnStatus.ApprovedAwaitingShipment;
        row.Histories.Add(History(from, CustomerReturnStatus.ApprovedAwaitingShipment,
            $"Approved. Shipping: {ReturnShippingShare.Label(shippingPayer)}.", memberId, null));
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

    public async Task<(bool Success, string? Error)> MarkReceivedAsync(
        int returnRequestId, int memberId, int? warehouseId = null,
        IReadOnlyList<CustomerReturnLineReceive>? lines = null,
        decimal? returnShippingCost = null, CancellationToken ct = default)
    {
        await using var context = await _db.CreateDbContextAsync(ct);
        var row = await context.CustomerReturnRequests
            .Include(r => r.Lines).ThenInclude(l => l.OrderItem)
            .Include(r => r.Order).ThenInclude(o => o.OrderItems)
            .FirstOrDefaultAsync(r => r.CustomerReturnRequestID == returnRequestId, ct);
        if (row is null) return (false, "Return not found.");

        var dropOff = row.ShippingPayer == (byte)ReturnShippingPayer.DropOffAtCenter;
        var allowed = dropOff
            ? new[] { (byte)CustomerReturnStatus.Shipped, (byte)CustomerReturnStatus.ApprovedAwaitingShipment }
            : new[] { (byte)CustomerReturnStatus.Shipped };
        if (!allowed.Contains(row.Status))
            return (false, "Invalid status for this action.");

        if (warehouseId is int wid && wid > 0)
        {
            var whOk = await context.Warehouses.AnyAsync(w => w.WarehouseID == wid && w.WebsiteID == row.WebsiteID, ct);
            if (!whOk) return (false, "Warehouse not found on this site.");
            row.ReceivedWarehouseID = wid;
            if (row.StockDocumentID is int docId)
            {
                var (whSet, whErr) = await _stockDocs.SetDestinationWarehouseAsync(docId, wid, memberId, ct);
                if (!whSet) return (false, whErr);
            }
        }

        if (returnShippingCost is decimal ship && row.ShippingPayer != (byte)ReturnShippingPayer.DropOffAtCenter)
            row.ReturnShippingCost = ship < 0 ? 0 : decimal.Round(ship, 4);

        if (lines is { Count: > 0 })
        {
            foreach (var recv in lines)
            {
                var line = row.Lines.FirstOrDefault(l => l.CustomerReturnRequestLineID == recv.CustomerReturnRequestLineID);
                if (line is null) continue;
                line.ReceivedCondition = recv.ReceivedCondition;
                line.HealthGrade = recv.ReceivedCondition is (byte)StockItemCondition.New or (byte)StockItemCondition.Defective
                    ? (byte)0
                    : recv.HealthGrade;
            }

            if (row.StockDocumentID is int sid)
            {
                var wms = await _stockDocs.GetByIdAsync(sid, ct);
                if (wms is not null)
                {
                    foreach (var line in row.Lines.Where(l => l.ProductVariantID is int && l.ReceivedCondition > 0))
                    {
                        var wmsLine = wms.StockDocumentLines.FirstOrDefault(sl => sl.ProductVariantID == line.ProductVariantID);
                        if (wmsLine is null) continue;
                        var (ok, err) = await _stockDocs.SetReturnLineConditionAsync(
                            wmsLine.StockDocumentLineID,
                            (StockItemCondition)line.ReceivedCondition,
                            (StockHealthGrade)line.HealthGrade,
                            memberId, ct);
                        if (!ok) return (false, err);
                    }
                }
            }
        }

        ApplyImpact(row);
        var from = row.Status;
        row.Status = (byte)CustomerReturnStatus.Received;
        row.UpdatedAt = DateTime.UtcNow;
        row.Histories.Add(History(from, CustomerReturnStatus.Received,
            warehouseId is int w ? $"Received at warehouse #{w}" : "Received at warehouse", memberId, null));
        await context.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> MarkCompletedAsync(int returnRequestId, int memberId, CancellationToken ct = default)
    {
        await using var context = await _db.CreateDbContextAsync(ct);
        using var ambient = AmbientDbContext.Use(context);
        var row = await context.CustomerReturnRequests
            .Include(r => r.Lines).ThenInclude(l => l.OrderItem)
            .Include(r => r.Order).ThenInclude(o => o.OrderItems)
            .FirstOrDefaultAsync(r => r.CustomerReturnRequestID == returnRequestId, ct);
        if (row is null) return (false, "Return not found.");
        var allowed = new[]
        {
            (byte)CustomerReturnStatus.Received,
            (byte)CustomerReturnStatus.Shipped,
            (byte)CustomerReturnStatus.ApprovedAwaitingShipment,
        };
        if (!allowed.Contains(row.Status))
            return (false, "Invalid status for this action.");

        ApplyImpact(row);
        var from = row.Status;
        row.Status = (byte)CustomerReturnStatus.Completed;
        row.UpdatedAt = DateTime.UtcNow;
        row.Histories.Add(History(from, CustomerReturnStatus.Completed,
            row.SiteImpactAmount < 0
                ? $"Completed. Site loss {decimal.Round(-row.SiteImpactAmount, 4)} {row.CurrencyCode}."
                : $"Completed. Site profit {decimal.Round(row.SiteImpactAmount, 4)} {row.CurrencyCode}.",
            memberId, null));
        await context.SaveChangesAsync(ct);

        if (row.ImpactPostedAt is null)
        {
            var vendorId = row.Order?.OrderItems
                .FirstOrDefault(i => i.VendorID is > 0)?.VendorID;
            var siteIsSeller = row.Lines.All(l => l.OrderItem?.VendorID is null or 0);
            var split = ReturnShippingShare.Split((ReturnShippingPayer)row.ShippingPayer, row.ReturnShippingCost, siteIsSeller);
            await _ledger.PostCustomerReturnImpactAsync(
                row.OrderID, row.CustomerReturnRequestID,
                row.SiteShippingShare, row.SiteImpactAmount,
                row.CurrencyCode, $"RMA #{row.CustomerReturnRequestID}",
                vendorId, split.SellerShare, memberId, ct);
            row.ImpactPostedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }

        return (true, null);
    }

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

    private static (bool Ok, string? Error, DateTime? Start, DateTime? End, bool WindowExpired) EvaluateWindow(Order order, Website website)
    {
        if (!website.ReturnsEnabled)
            return (false, "Returns are disabled for this site.", null, null, false);
        if (order.Status is (byte)OrderStatus.Cancelled)
            return (false, "Cancelled orders cannot be returned.", null, null, false);
        if (order.PaidAt is null)
            return (false, "Only paid orders can be returned.", null, null, false);

        DateTime? start = null;
        if (website.ReturnWindowFrom == (byte)ReturnWindowFrom.Delivered)
        {
            if (order.ShippingStatus != (byte)OrderShippingStatus.Delivered)
                return (false, "Return window starts after the order is marked delivered.", null, null, false);
            start = order.ShippedAt ?? order.PaidAt;
        }
        else
        {
            if (order.ShippedAt is null && order.ShippingStatus < (byte)OrderShippingStatus.Shipped)
                return (false, "Return window starts after the order is shipped.", null, null, false);
            start = order.ShippedAt ?? order.PaidAt;
        }

        var days = website.ReturnWindowDays < 0 ? 0 : website.ReturnWindowDays;
        var end = start!.Value.AddDays(days);
        if (DateTime.UtcNow > end)
            return (true, $"Return window ({days} days) has expired.", start, end, true);
        return (true, null, start, end, false);
    }

    private static void ApplyImpact(CustomerReturnRequest row)
    {
        var siteIsSeller = row.Lines.All(l => l.OrderItem?.VendorID is null or 0);
        var (siteShare, _, _) = ReturnShippingShare.Split(
            (ReturnShippingPayer)row.ShippingPayer, row.ReturnShippingCost, siteIsSeller);
        row.SiteShippingShare = siteShare;

        decimal recovered = 0;
        foreach (var line in row.Lines)
        {
            if (line.ReceivedCondition == (byte)StockItemCondition.Defective)
                continue;
            recovered += line.UnitCost * line.Quantity;
        }
        row.RecoveredInventoryValue = decimal.Round(recovered, 4);
        row.SiteImpactAmount = decimal.Round(row.RecoveredInventoryValue - row.ApprovedRefundTotal - row.SiteShippingShare, 4);
    }

    private static decimal UnitPaid(OrderItem i) =>
        i.Quantity > 0 ? decimal.Round(i.TotalPrice / i.Quantity, 4) : i.UnitPrice;

    private static decimal UnitCostLocal(OrderItem item)
    {
        if (item.UnitCostUsd <= 0) return 0;
        if (item.UnitPriceUsd > 0 && item.UnitPrice > 0)
            return decimal.Round(item.UnitCostUsd * (item.UnitPrice / item.UnitPriceUsd), 4);
        return item.UnitCostUsd;
    }

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
        ReturnShippingCost = r.ReturnShippingCost,
        SiteShippingShare = r.SiteShippingShare,
        RecoveredInventoryValue = r.RecoveredInventoryValue,
        SiteImpactAmount = r.SiteImpactAmount,
        AcceptedAfterWindowExpired = r.AcceptedAfterWindowExpired,
        ReceivedWarehouseID = r.ReceivedWarehouseID,
        ReceivedWarehouseName = r.ReceivedWarehouse?.Name,
        CurrencyCode = r.CurrencyCode,
        RequestedRefundTotal = r.RequestedRefundTotal,
        ApprovedRefundTotal = r.ApprovedRefundTotal,
        ReviewNote = r.ReviewNote,
        ReviewedAt = r.ReviewedAt,
        ShippedAt = r.ShippedAt,
        ImpactPostedAt = r.ImpactPostedAt,
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
            UnitCost = l.UnitCost,
            UnitRefundRequested = l.UnitRefundRequested,
            UnitRefundApproved = l.UnitRefundApproved,
            ReceivedCondition = l.ReceivedCondition,
            HealthGrade = l.HealthGrade,
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
