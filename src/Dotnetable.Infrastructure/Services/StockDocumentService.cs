using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class StockDocumentService : IStockDocumentService
{
    private readonly AppDbContext _context;
    private readonly IWarehouseService _warehouses;
    private readonly IInventoryService _inventory;

    public StockDocumentService(AppDbContext context, IWarehouseService warehouses, IInventoryService inventory)
    {
        _context = context;
        _warehouses = warehouses;
        _inventory = inventory;
    }

    public async Task<PagedResult<StockDocument>> GetPagedAsync(int websiteId, byte? status, byte? type, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.StockDocuments.AsNoTracking()
            .Include(d => d.FromWarehouse).Include(d => d.ToWarehouse)
            .Where(d => d.WebsiteID == websiteId);
        if (status is byte s) q = q.Where(d => d.Status == s);
        if (type is byte t) q = q.Where(d => d.DocumentType == t);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(d => d.CreatedAt).Skip(query.Skip).Take(query.Take).ToListAsync(ct);
        return new PagedResult<StockDocument> { Items = items, TotalCount = total };
    }

    public async Task<StockDocument?> GetByIdAsync(int documentId, CancellationToken ct = default) =>
        await _context.StockDocuments.AsNoTracking()
            .Include(d => d.StockDocumentLines).ThenInclude(l => l.ProductVariant).ThenInclude(v => v.Product)
            .Include(d => d.FromWarehouse).Include(d => d.ToWarehouse)
            .Include(d => d.StockDocumentHistories)
            .FirstOrDefaultAsync(d => d.StockDocumentID == documentId, ct);

    public async Task<(bool Success, string? Error, StockDocument? Doc)> CreateAsync(
        int websiteId, StockDocumentType type, int? fromWarehouseId, int? toWarehouseId,
        int? supplierId, string? note, IReadOnlyList<StockDocumentLineRequest> lines, int? memberId, CancellationToken ct = default)
    {
        await _warehouses.EnsureDefaultAsync(websiteId, ct);
        if (lines.Count == 0 || lines.Any(l => l.Quantity <= 0))
            return (false, "At least one line with positive quantity is required.", null);

        if (type is StockDocumentType.Inbound or StockDocumentType.Adjustment or StockDocumentType.Count)
            toWarehouseId ??= await DefaultWarehouseId(websiteId, ct);
        if (type is StockDocumentType.Outbound)
            fromWarehouseId ??= await DefaultWarehouseId(websiteId, ct);
        if (type == StockDocumentType.Transfer)
        {
            fromWarehouseId ??= await DefaultWarehouseId(websiteId, ct);
            if (toWarehouseId is null || toWarehouseId == fromWarehouseId)
                return (false, "Transfer requires a different destination warehouse.", null);
        }

        var seq = await _context.StockDocuments.CountAsync(d => d.WebsiteID == websiteId, ct) + 1;
        var doc = new StockDocument
        {
            WebsiteID = websiteId,
            DocumentNumber = $"STK-{DateTime.UtcNow:yyyyMM}-{seq:D5}",
            DocumentType = (byte)type,
            Status = (byte)StockDocumentStatus.Draft,
            FromWarehouseID = fromWarehouseId,
            ToWarehouseID = toWarehouseId,
            SupplierID = supplierId,
            Note = note,
            RequestedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
        };
        foreach (var l in lines)
        {
            doc.StockDocumentLines.Add(new StockDocumentLine
            {
                ProductVariantID = l.ProductVariantID,
                Quantity = l.Quantity,
                UnitCost = l.UnitCost,
                UnitCostUsd = l.UnitCost,
                Note = l.Note,
            });
        }
        _context.StockDocuments.Add(doc);
        AddHistory(doc, 0, (byte)StockDocumentStatus.Draft, "Created", memberId);
        await _context.SaveChangesAsync(ct);
        return (true, null, doc);
    }

    public Task<(bool Success, string? Error)> SubmitAsync(int documentId, int? memberId, CancellationToken ct = default) =>
        TransitionAsync(documentId, StockDocumentStatus.Draft, StockDocumentStatus.Submitted, memberId, "Submitted", ct);

    public Task<(bool Success, string? Error)> ApproveAsync(int documentId, int? memberId, CancellationToken ct = default) =>
        TransitionAsync(documentId, StockDocumentStatus.Submitted, StockDocumentStatus.Approved, memberId, "Approved", ct, approve: true);

    public Task<(bool Success, string? Error)> RejectAsync(int documentId, int? memberId, string? note, CancellationToken ct = default) =>
        TransitionAsync(documentId, StockDocumentStatus.Submitted, StockDocumentStatus.Rejected, memberId, note ?? "Rejected", ct);

    public Task<(bool Success, string? Error)> CancelAsync(int documentId, int? memberId, string? note, CancellationToken ct = default) =>
        TransitionAsync(documentId, null, StockDocumentStatus.Cancelled, memberId, note ?? "Cancelled", ct, allowFrom:
            [(byte)StockDocumentStatus.Draft, (byte)StockDocumentStatus.Submitted, (byte)StockDocumentStatus.Approved]);

    public async Task<(bool Success, string? Error)> PostAsync(int documentId, int? memberId, CancellationToken ct = default)
    {
        var doc = await _context.StockDocuments
            .Include(d => d.StockDocumentLines)
            .FirstOrDefaultAsync(d => d.StockDocumentID == documentId, ct);
        if (doc is null) return (false, "Document not found.");
        if (doc.Status is not ((byte)StockDocumentStatus.Approved or (byte)StockDocumentStatus.Submitted))
            return (false, "Only approved (or submitted) documents can be posted.");

        var type = (StockDocumentType)doc.DocumentType;
        try
        {
            foreach (var line in doc.StockDocumentLines)
            {
                switch (type)
                {
                    case StockDocumentType.Inbound:
                    case StockDocumentType.Count:
                    case StockDocumentType.Adjustment when line.Quantity > 0:
                        await AdjustWarehouseAsync(doc.ToWarehouseID ?? doc.FromWarehouseID, line.ProductVariantID, Math.Abs(line.Quantity), ct);
                        await _inventory.AdjustAsync(doc.WebsiteID, line.ProductVariantID, Math.Abs(line.Quantity), line.UnitCost,
                            $"Stock doc {doc.DocumentNumber}", memberId ?? 0, ct);
                        break;
                    case StockDocumentType.Outbound:
                    case StockDocumentType.Adjustment when line.Quantity < 0:
                        await AdjustWarehouseAsync(doc.FromWarehouseID, line.ProductVariantID, -Math.Abs(line.Quantity), ct);
                        await _inventory.AdjustAsync(doc.WebsiteID, line.ProductVariantID, -Math.Abs(line.Quantity), line.UnitCost,
                            $"Stock doc {doc.DocumentNumber}", memberId ?? 0, ct);
                        break;
                    case StockDocumentType.Transfer:
                        await AdjustWarehouseAsync(doc.FromWarehouseID, line.ProductVariantID, -line.Quantity, ct);
                        await AdjustWarehouseAsync(doc.ToWarehouseID, line.ProductVariantID, line.Quantity, ct);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }

        var from = doc.Status;
        doc.Status = (byte)StockDocumentStatus.Posted;
        doc.PostedAt = DateTime.UtcNow;
        doc.PostedByMemberID = memberId;
        AddHistory(doc, from, (byte)StockDocumentStatus.Posted, "Posted", memberId);
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    private async Task<(bool Success, string? Error)> TransitionAsync(
        int documentId, StockDocumentStatus? requiredFrom, StockDocumentStatus to, int? memberId, string note,
        CancellationToken ct, bool approve = false, byte[]? allowFrom = null)
    {
        var doc = await _context.StockDocuments.FirstOrDefaultAsync(d => d.StockDocumentID == documentId, ct);
        if (doc is null) return (false, "Document not found.");
        if (doc.Status == (byte)StockDocumentStatus.Posted)
            return (false, "Posted documents cannot change status.");
        if (allowFrom is not null)
        {
            if (!allowFrom.Contains(doc.Status)) return (false, "Invalid status transition.");
        }
        else if (requiredFrom is StockDocumentStatus req && doc.Status != (byte)req)
            return (false, $"Document must be {req}.");

        var from = doc.Status;
        doc.Status = (byte)to;
        if (to == StockDocumentStatus.Submitted) doc.SubmittedAt = DateTime.UtcNow;
        if (approve || to == StockDocumentStatus.Approved)
        {
            doc.ApprovedAt = DateTime.UtcNow;
            doc.ApprovedByMemberID = memberId;
        }
        AddHistory(doc, from, (byte)to, note, memberId);
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    private async Task AdjustWarehouseAsync(int? warehouseId, int variantId, int delta, CancellationToken ct)
    {
        if (warehouseId is null or <= 0) throw new InvalidOperationException("Warehouse is required.");
        var stock = await _context.WarehouseStocks
            .FirstOrDefaultAsync(s => s.WarehouseID == warehouseId && s.ProductVariantID == variantId, ct);
        if (stock is null)
        {
            if (delta < 0) throw new InvalidOperationException("Insufficient warehouse stock.");
            stock = new WarehouseStock
            {
                WarehouseID = warehouseId.Value,
                ProductVariantID = variantId,
                QuantityOnHand = delta,
                QuantityReserved = 0,
                RowVersion = Array.Empty<byte>(),
            };
            _context.WarehouseStocks.Add(stock);
        }
        else
        {
            var next = stock.QuantityOnHand + delta;
            if (next < 0) throw new InvalidOperationException("Insufficient warehouse stock.");
            stock.QuantityOnHand = next;
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> WebsiteHasWarehouseAsync(int websiteId, CancellationToken ct = default)
    {
        if (websiteId <= 0) return false;
        // Do not auto-create: WMS order path only when the site already has a warehouse row.
        return await _context.Warehouses.AsNoTracking()
            .AnyAsync(w => w.WebsiteID == websiteId && w.IsActive, ct);
    }

    public Task<StockDocument?> GetOutboundForOrderAsync(int orderId, CancellationToken ct = default) =>
        _context.StockDocuments.AsNoTracking()
            .Where(d => d.OrderID == orderId
                && d.DocumentType == (byte)StockDocumentType.Outbound
                && d.Status != (byte)StockDocumentStatus.Cancelled
                && d.Status != (byte)StockDocumentStatus.Rejected)
            .OrderByDescending(d => d.StockDocumentID)
            .FirstOrDefaultAsync(ct);

    public async Task<(bool Success, string? Error, StockDocument? Doc)> EnsureOutboundForOrderAsync(
        int orderId, int? memberId, CancellationToken ct = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems).ThenInclude(i => i.ProductVariant).ThenInclude(v => v!.Product)
            .FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
        if (order is null) return (false, "Order not found.", null);

        var existing = await _context.StockDocuments
            .Include(d => d.StockDocumentLines)
            .FirstOrDefaultAsync(d =>
                d.OrderID == orderId
                && d.DocumentType == (byte)StockDocumentType.Outbound
                && d.Status != (byte)StockDocumentStatus.Cancelled
                && d.Status != (byte)StockDocumentStatus.Rejected, ct);
        if (existing is not null)
            return (true, null, existing);

        if (!await WebsiteHasWarehouseAsync(order.WebsiteID, ct))
            return (true, null, null); // No WMS — caller keeps legacy inventory path.

        var physical = order.OrderItems
            .Where(i => i.ProductVariantID is int)
            .Where(i =>
            {
                var p = i.ProductVariant?.Product;
                if (p is null) return true; // variant free-form with id only: treat as stockable
                return p.RequiresShipping || p.ProductType == (byte)ProductType.Physical;
            })
            .GroupBy(i => i.ProductVariantID!.Value)
            .Select(g => new StockDocumentLineRequest
            {
                ProductVariantID = g.Key,
                Quantity = g.Sum(x => x.Quantity),
                UnitCost = g.Max(x => x.UnitCostUsd),
                Note = g.First().SkuSnapshot,
            })
            .Where(l => l.Quantity > 0)
            .ToList();

        if (physical.Count == 0)
            return (true, null, null); // Digital-only order.

        var fromWh = await DefaultWarehouseId(order.WebsiteID, ct);
        var seq = await _context.StockDocuments.CountAsync(d => d.WebsiteID == order.WebsiteID, ct) + 1;
        var doc = new StockDocument
        {
            WebsiteID = order.WebsiteID,
            DocumentNumber = $"OUT-{order.OrderNumber}",
            DocumentType = (byte)StockDocumentType.Outbound,
            Status = (byte)StockDocumentStatus.Submitted,
            FromWarehouseID = fromWh,
            OrderID = order.OrderID,
            Note = $"Auto pick for order {order.OrderNumber}",
            RequestedByMemberID = memberId,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };
        // Unique-ish number if collision
        if (await _context.StockDocuments.AnyAsync(d => d.WebsiteID == order.WebsiteID && d.DocumentNumber == doc.DocumentNumber, ct))
            doc.DocumentNumber = $"OUT-{order.OrderNumber}-{seq:D4}";

        foreach (var l in physical)
        {
            doc.StockDocumentLines.Add(new StockDocumentLine
            {
                ProductVariantID = l.ProductVariantID,
                Quantity = l.Quantity,
                UnitCost = l.UnitCost,
                UnitCostUsd = l.UnitCost,
                Note = l.Note,
            });
        }

        _context.StockDocuments.Add(doc);
        AddHistory(doc, 0, (byte)StockDocumentStatus.Submitted, $"Created from order {order.OrderNumber}", memberId);
        await _context.SaveChangesAsync(ct);
        return (true, null, doc);
    }

    public async Task<(bool Success, string? Error)> PostOutboundForOrderAsync(int orderId, int? memberId, CancellationToken ct = default)
    {
        var ensure = await EnsureOutboundForOrderAsync(orderId, memberId, ct);
        if (!ensure.Success) return (false, ensure.Error);
        if (ensure.Doc is null) return (true, null); // No WMS / no physical lines.

        var doc = ensure.Doc;
        if (doc.Status == (byte)StockDocumentStatus.Posted)
            return (true, null);

        if (doc.Status == (byte)StockDocumentStatus.Draft)
        {
            var submit = await SubmitAsync(doc.StockDocumentID, memberId, ct);
            if (!submit.Success) return submit;
            doc = (await GetByIdAsync(doc.StockDocumentID, ct))!;
        }

        if (doc.Status == (byte)StockDocumentStatus.Submitted)
        {
            var approve = await ApproveAsync(doc.StockDocumentID, memberId, ct);
            if (!approve.Success) return approve;
        }

        // Re-load status after transitions
        var current = await _context.StockDocuments.AsNoTracking()
            .FirstOrDefaultAsync(d => d.StockDocumentID == doc.StockDocumentID, ct);
        if (current is null) return (false, "Outbound document not found.");
        if (current.Status == (byte)StockDocumentStatus.Posted)
            return (true, null);

        return await PostAsync(current.StockDocumentID, memberId, ct);
    }

    private async Task<int> DefaultWarehouseId(int websiteId, CancellationToken ct)
    {
        await _warehouses.EnsureDefaultAsync(websiteId, ct);
        return await _context.Warehouses.Where(w => w.WebsiteID == websiteId && w.IsDefault)
            .Select(w => w.WarehouseID).FirstAsync(ct);
    }

    private void AddHistory(StockDocument doc, byte from, byte to, string? note, int? memberId) =>
        _context.StockDocumentHistories.Add(new StockDocumentHistory
        {
            StockDocument = doc,
            FromStatus = from,
            ToStatus = to,
            Note = note,
            CreatedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
        });
}
