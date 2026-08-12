using Dotnetable.Application.Authorization;
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
    private readonly AppDbContext _fallback;
    private AppDbContext _context => AmbientDbContext.Current ?? _fallback;
    private readonly IWarehouseService _warehouses;
    private readonly IInventoryService _inventory;
    private readonly IVendorProductService _vendorProducts;
    private readonly IAdminNotificationService _notifications;
    private readonly IFinancialLedgerService _ledger;

    public StockDocumentService(
        AppDbContext context,
        IWarehouseService warehouses,
        IInventoryService inventory,
        IVendorProductService vendorProducts,
        IAdminNotificationService notifications,
        IFinancialLedgerService ledger)
    {
        _fallback = context;
        _warehouses = warehouses;
        _inventory = inventory;
        _vendorProducts = vendorProducts;
        _notifications = notifications;
        _ledger = ledger;
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

        if (type is StockDocumentType.Inbound or StockDocumentType.Adjustment or StockDocumentType.Count or StockDocumentType.Return)
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
            var cond = type == StockDocumentType.Return
                ? (l.ReturnCondition == 0 ? (byte)StockItemCondition.New : l.ReturnCondition)
                : (byte)StockItemCondition.None;
            var grade = type == StockDocumentType.Return ? l.HealthGrade : (byte)0;
            if (type == StockDocumentType.Return
                && cond is not ((byte)StockItemCondition.New or (byte)StockItemCondition.None or (byte)StockItemCondition.Defective)
                && grade == 0)
                return (false, "Health grade is required for non-new return lines.", null);

            int book = 0;
            int? counted = null;
            int qty = l.Quantity;
            if (type == StockDocumentType.Count)
            {
                var whId = toWarehouseId ?? fromWarehouseId;
                book = l.BookQuantity > 0
                    ? l.BookQuantity
                    : await GetWarehouseOnHandAsync(whId, l.ProductVariantID, ct);
                counted = l.CountedQuantity ?? l.Quantity;
                qty = counted.Value - book; // variance for post
            }

            doc.StockDocumentLines.Add(new StockDocumentLine
            {
                ProductVariantID = l.ProductVariantID,
                Quantity = qty,
                BookQuantity = book,
                CountedQuantity = counted,
                UnitCost = l.UnitCost,
                UnitCostUsd = l.UnitCost,
                Note = l.Note,
                ReturnCondition = cond,
                HealthGrade = grade,
            });
        }
        _context.StockDocuments.Add(doc);
        AddHistory(doc, 0, (byte)StockDocumentStatus.Draft, "Created", memberId);
        await _context.SaveChangesAsync(ct);
        return (true, null, doc);
    }

    public async Task<(bool Success, string? Error)> SubmitAsync(int documentId, int? memberId, CancellationToken ct = default)
    {
        var result = await TransitionAsync(documentId, StockDocumentStatus.Draft, StockDocumentStatus.Submitted, memberId, "Submitted", ct);
        if (result.Success) await NotifyWarehouseAsync(documentId, "submitted", ct);
        return result;
    }

    public async Task<(bool Success, string? Error)> ApproveAsync(int documentId, int? memberId, CancellationToken ct = default)
    {
        var result = await TransitionAsync(documentId, StockDocumentStatus.Submitted, StockDocumentStatus.Approved, memberId, "Approved / ready to pick", ct, approve: true);
        if (result.Success) await NotifyWarehouseAsync(documentId, "approved / ready", ct);
        return result;
    }

    public Task<(bool Success, string? Error)> MarkReadyToPickAsync(int documentId, int? memberId, CancellationToken ct = default) =>
        ApproveAsync(documentId, memberId, ct);

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
                    case StockDocumentType.Adjustment when line.Quantity > 0:
                        await AdjustWarehouseAsync(doc.ToWarehouseID ?? doc.FromWarehouseID, line.ProductVariantID, Math.Abs(line.Quantity), clearReserved: 0, ct);
                        await _inventory.AdjustAsync(doc.WebsiteID, line.ProductVariantID, Math.Abs(line.Quantity), line.UnitCost,
                            $"Stock doc {doc.DocumentNumber}", memberId ?? 0, ct);
                        await SyncInventoryFromWarehouseAsync(doc.WebsiteID, line.ProductVariantID, ct);
                        break;
                    case StockDocumentType.Count:
                        // Quantity is variance (Counted − Book). Zero variance = no stock change.
                        if (line.Quantity != 0)
                        {
                            await AdjustWarehouseAsync(doc.ToWarehouseID ?? doc.FromWarehouseID, line.ProductVariantID, line.Quantity, clearReserved: 0, ct);
                            await _inventory.AdjustAsync(doc.WebsiteID, line.ProductVariantID, line.Quantity, line.UnitCost,
                                $"Count {doc.DocumentNumber} variance {line.Quantity} (book {line.BookQuantity} → counted {line.CountedQuantity})",
                                memberId ?? 0, ct);
                            await SyncInventoryFromWarehouseAsync(doc.WebsiteID, line.ProductVariantID, ct);
                        }
                        break;
                    case StockDocumentType.Outbound:
                    case StockDocumentType.Adjustment when line.Quantity < 0:
                        await AdjustWarehouseAsync(doc.FromWarehouseID, line.ProductVariantID, -Math.Abs(line.Quantity),
                            clearReserved: Math.Abs(line.Quantity), ct);
                        // Clear reservation + on-hand (sale) when this is an order pick; otherwise adjustment.
                        if (doc.OrderID is not null)
                        {
                            await _inventory.DecrementOnFulfillAsync(doc.WebsiteID, line.ProductVariantID, Math.Abs(line.Quantity),
                                doc.OrderID, null, memberId, ct);
                        }
                        else
                        {
                            await _inventory.AdjustAsync(doc.WebsiteID, line.ProductVariantID, -Math.Abs(line.Quantity), line.UnitCost,
                                $"Stock doc {doc.DocumentNumber}", memberId ?? 0, ct);
                        }
                        await SyncInventoryFromWarehouseAsync(doc.WebsiteID, line.ProductVariantID, ct);
                        break;
                    case StockDocumentType.Transfer:
                        if (line.Quantity <= 0)
                            throw new InvalidOperationException("Transfer quantity must be positive.");
                        await AdjustWarehouseAsync(doc.FromWarehouseID, line.ProductVariantID, -line.Quantity, clearReserved: 0, ct);
                        await AdjustWarehouseAsync(doc.ToWarehouseID, line.ProductVariantID, line.Quantity, clearReserved: 0, ct);
                        await SyncInventoryFromWarehouseAsync(doc.WebsiteID, line.ProductVariantID, ct);
                        break;
                    case StockDocumentType.Return:
                        await PostReturnLineAsync(doc, line, memberId, ct);
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

        // Align GL inventory with warehouse: COGS on outbound, reverse COGS on sellable return.
        if (doc.OrderID is int cogsOrderId)
        {
            try
            {
                if (type == StockDocumentType.Outbound)
                    await _ledger.PostInventoryCogsForOrderAsync(cogsOrderId, doc.StockDocumentID, memberId, ct);
                else if (type == StockDocumentType.Return)
                    await _ledger.PostInventoryCogsReversalForReturnAsync(cogsOrderId, doc.StockDocumentID, memberId, ct);
            }
            catch
            {
                /* never break stock post for GL */
            }
        }

        await NotifyWarehouseAsync(documentId, "posted", ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error, StockDocument? Doc)> CreateCountFromWarehouseAsync(
        int websiteId, int warehouseId, string? note, int? memberId, CancellationToken ct = default)
    {
        if (websiteId <= 0 || warehouseId <= 0) return (false, "Website and warehouse are required.", null);
        var stocks = await _warehouses.GetStockAsync(warehouseId, ct);
        var lines = stocks
            .Where(s => s.QuantityOnHand != 0 || s.QuantityReserved != 0)
            .Select(s => new StockDocumentLineRequest
            {
                ProductVariantID = s.ProductVariantID,
                Quantity = s.QuantityOnHand, // counted defaults to book
                BookQuantity = s.QuantityOnHand,
                CountedQuantity = s.QuantityOnHand,
                UnitCost = 0,
                Note = s.ProductVariant?.Sku,
            })
            .ToList();
        if (lines.Count == 0)
            return (false, "Warehouse has no stock rows to count. Add stock via inbound first, or create a count line manually.", null);

        return await CreateAsync(websiteId, StockDocumentType.Count, null, warehouseId, null,
            note ?? "Physical count", lines, memberId, ct);
    }

    public async Task<(bool Success, string? Error)> SetCountedQuantityAsync(
        int stockDocumentLineId, int countedQuantity, int? memberId, CancellationToken ct = default)
    {
        if (countedQuantity < 0) return (false, "Counted quantity cannot be negative.");
        var line = await _context.StockDocumentLines
            .Include(l => l.StockDocument)
            .FirstOrDefaultAsync(l => l.StockDocumentLineID == stockDocumentLineId, ct);
        if (line is null) return (false, "Line not found.");
        if (line.StockDocument.DocumentType != (byte)StockDocumentType.Count)
            return (false, "Only count document lines have counted quantity.");
        if (line.StockDocument.Status == (byte)StockDocumentStatus.Posted)
            return (false, "Cannot edit after post.");

        line.CountedQuantity = countedQuantity;
        line.Quantity = countedQuantity - line.BookQuantity; // variance
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> AddLineAsync(
        int documentId, StockDocumentLineRequest line, int? memberId, CancellationToken ct = default)
    {
        if (line.ProductVariantID <= 0 || line.Quantity <= 0 && line.CountedQuantity is null)
            return (false, "Variant and quantity are required.");
        var doc = await _context.StockDocuments
            .Include(d => d.StockDocumentLines)
            .FirstOrDefaultAsync(d => d.StockDocumentID == documentId, ct);
        if (doc is null) return (false, "Document not found.");
        if (doc.Status is (byte)StockDocumentStatus.Posted or (byte)StockDocumentStatus.Cancelled or (byte)StockDocumentStatus.Rejected)
            return (false, "Cannot add lines to this document.");

        var type = (StockDocumentType)doc.DocumentType;
        int book = 0;
        int? counted = null;
        int qty = line.Quantity;
        if (type == StockDocumentType.Count)
        {
            book = line.BookQuantity > 0
                ? line.BookQuantity
                : await GetWarehouseOnHandAsync(doc.ToWarehouseID ?? doc.FromWarehouseID, line.ProductVariantID, ct);
            counted = line.CountedQuantity ?? line.Quantity;
            qty = counted.Value - book;
        }

        doc.StockDocumentLines.Add(new StockDocumentLine
        {
            ProductVariantID = line.ProductVariantID,
            Quantity = qty,
            BookQuantity = book,
            CountedQuantity = counted,
            UnitCost = line.UnitCost,
            UnitCostUsd = line.UnitCost,
            Note = line.Note,
            ReturnCondition = type == StockDocumentType.Return
                ? (line.ReturnCondition == 0 ? (byte)StockItemCondition.New : line.ReturnCondition)
                : (byte)0,
            HealthGrade = line.HealthGrade,
        });
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    private async Task<int> GetWarehouseOnHandAsync(int? warehouseId, int variantId, CancellationToken ct)
    {
        if (warehouseId is null or <= 0) return 0;
        return await _context.WarehouseStocks.AsNoTracking()
            .Where(s => s.WarehouseID == warehouseId && s.ProductVariantID == variantId)
            .Select(s => s.QuantityOnHand)
            .FirstOrDefaultAsync(ct);
    }

    private async Task NotifyWarehouseAsync(int documentId, string action, CancellationToken ct)
    {
        try
        {
            var doc = await _context.StockDocuments.AsNoTracking()
                .FirstOrDefaultAsync(d => d.StockDocumentID == documentId, ct);
            if (doc is null) return;
            var type = (StockDocumentType)doc.DocumentType;
            await _notifications.NotifyRoleAsync(
                doc.WebsiteID,
                [RoleKeys.WarehouseView, RoleKeys.WarehouseIssue, RoleKeys.WarehouseReceive, RoleKeys.WarehousePost],
                AdminNotificationType.WarehouseDocument,
                $"Warehouse {type} {action}",
                $"{doc.DocumentNumber} ({type}) was {action}.",
                $"/inventory/stock-documents/{doc.StockDocumentID}",
                doc.StockDocumentID,
                ct);
        }
        catch
        {
            /* never fail business flow on notify */
        }
    }

    private async Task PostReturnLineAsync(StockDocument doc, StockDocumentLine line, int? memberId, CancellationToken ct)
    {
        var condition = (StockItemCondition)line.ReturnCondition;
        if (condition == StockItemCondition.None)
            condition = StockItemCondition.New;

        if (condition is not (StockItemCondition.New or StockItemCondition.Defective)
            && line.HealthGrade == 0)
            throw new InvalidOperationException(
                "Health grade is required for non-new returns (LikeNew / OpenBox / Display / Used).");

        var whId = doc.ToWarehouseID ?? doc.FromWarehouseID;
        // Always receive into warehouse (returned goods location = default warehouse).
        await AdjustWarehouseAsync(whId, line.ProductVariantID, Math.Abs(line.Quantity), clearReserved: 0, ct);

        if (condition == StockItemCondition.Defective)
        {
            // Scrap path: physical only — no sellable inventory / catalog restore.
            return;
        }

        var qty = Math.Abs(line.Quantity);
        var noteTag = $"{condition}" + (line.HealthGrade > 0 ? $"/G{line.HealthGrade}" : "");
        await _inventory.RestockReturnAsync(doc.WebsiteID, line.ProductVariantID, qty, line.UnitCost,
            $"Return {doc.DocumentNumber} ({noteTag})", memberId ?? 0, ct);

        // New (or legacy Sellable): restore original listing.
        // Pre-owned conditions: restock into used-channel listing (condition + health grade).
        var isNew = condition is StockItemCondition.New;
        if (doc.OrderID is int orderId)
        {
            var orderItems = await _context.OrderItems.AsNoTracking()
                .Where(i => i.OrderID == orderId && i.ProductVariantID == line.ProductVariantID && i.VendorProductID != null)
                .Select(i => new { i.VendorProductID, i.VendorID, i.Quantity })
                .ToListAsync(ct);
            var remaining = qty;
            foreach (var oi in orderItems)
            {
                if (remaining <= 0) break;
                var take = Math.Min(remaining, oi.Quantity);
                if (isNew)
                    await _vendorProducts.RestockAsync(oi.VendorProductID!.Value, take, ct);
                else
                {
                    var vendorId = oi.VendorID
                        ?? await _context.VendorProducts.AsNoTracking()
                            .Where(vp => vp.VendorProductID == oi.VendorProductID)
                            .Select(vp => (int?)vp.VendorID).FirstOrDefaultAsync(ct);
                    if (vendorId is int vid)
                    {
                        await _vendorProducts.RestockWithConditionAsync(
                            doc.WebsiteID, vid, line.ProductVariantID, take,
                            (byte)condition, line.HealthGrade, ct);
                        await EnsureUsedGoodsCategoryAsync(doc.WebsiteID, line.ProductVariantID, ct);
                    }
                }
                remaining -= take;
            }
            // Leftover without vendor: still try first site vendor for used channel.
            if (remaining > 0 && !isNew)
            {
                var vendorId = await _context.VendorProducts.AsNoTracking()
                    .Where(vp => vp.WebsiteID == doc.WebsiteID && vp.ProductVariantID == line.ProductVariantID)
                    .Select(vp => (int?)vp.VendorID).FirstOrDefaultAsync(ct)
                    ?? await _context.Vendors.AsNoTracking()
                        .Where(v => v.WebsiteID == doc.WebsiteID)
                        .Select(v => (int?)v.VendorID).FirstOrDefaultAsync(ct);
                if (vendorId is int vid)
                {
                    await _vendorProducts.RestockWithConditionAsync(
                        doc.WebsiteID, vid, line.ProductVariantID, remaining,
                        (byte)condition, line.HealthGrade, ct);
                    await EnsureUsedGoodsCategoryAsync(doc.WebsiteID, line.ProductVariantID, ct);
                }
            }
            await _vendorProducts.SyncInventoryOnHandFromListingsAsync(doc.WebsiteID, line.ProductVariantID, ct);
        }
        else
        {
            await SyncInventoryFromWarehouseAsync(doc.WebsiteID, line.ProductVariantID, ct);
        }
    }

    /// <summary>Ensure product is mapped to site "used-goods" category so non-new stock is discoverable.</summary>
    private async Task EnsureUsedGoodsCategoryAsync(int websiteId, int productVariantId, CancellationToken ct)
    {
        const string slug = "used-goods";
        var cat = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.WebsiteID == websiteId && c.Slug == slug, ct);
        if (cat is null)
        {
            cat = new ProductCategory
            {
                WebsiteID = websiteId,
                Name = "Used / refurbished",
                Slug = slug,
                SortOrder = 900,
                IsActive = true,
            };
            _context.ProductCategories.Add(cat);
            await _context.SaveChangesAsync(ct);
        }

        var productId = await _context.ProductVariants.AsNoTracking()
            .Where(v => v.ProductVariantID == productVariantId)
            .Select(v => v.ProductID)
            .FirstOrDefaultAsync(ct);
        if (productId <= 0) return;

        var mapped = await _context.ProductCategoryMaps
            .AnyAsync(m => m.ProductID == productId && m.ProductCategoryID == cat.ProductCategoryID, ct);
        if (!mapped)
        {
            _context.ProductCategoryMaps.Add(new ProductCategoryMap
            {
                ProductID = productId,
                ProductCategoryID = cat.ProductCategoryID,
                IsPrimary = false,
            });
            await _context.SaveChangesAsync(ct);
        }
    }

    private async Task SyncInventoryFromWarehouseAsync(int websiteId, int variantId, CancellationToken ct)
    {
        var sum = await _warehouses.SumOnHandForVariantAsync(websiteId, variantId, ct);
        await _inventory.SyncOnHandFromWarehousesAsync(websiteId, variantId, sum, ct);
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

    private async Task AdjustWarehouseAsync(int? warehouseId, int variantId, int delta, int clearReserved, CancellationToken ct)
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
            if (clearReserved > 0)
                stock.QuantityReserved = Math.Max(0, stock.QuantityReserved - clearReserved);
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> WebsiteHasWarehouseAsync(int websiteId, CancellationToken ct = default)
    {
        if (websiteId <= 0) return false;
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

    public Task<StockDocument?> GetReturnForOrderAsync(int orderId, CancellationToken ct = default) =>
        _context.StockDocuments.AsNoTracking()
            .Where(d => d.OrderID == orderId
                && d.DocumentType == (byte)StockDocumentType.Return
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
            return (true, null, null);

        var physical = BuildPhysicalLines(order);
        if (physical.Count == 0)
            return (true, null, null);

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
                ReturnCondition = 0,
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
        if (ensure.Doc is null) return (true, null);

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

        var current = await _context.StockDocuments.AsNoTracking()
            .FirstOrDefaultAsync(d => d.StockDocumentID == doc.StockDocumentID, ct);
        if (current is null) return (false, "Outbound document not found.");
        if (current.Status == (byte)StockDocumentStatus.Posted)
            return (true, null);

        return await PostAsync(current.StockDocumentID, memberId, ct);
    }

    public async Task<(bool CanShip, string? Error)> CanShipOrderAsync(int orderId, CancellationToken ct = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems).ThenInclude(i => i.ProductVariant).ThenInclude(v => v!.Product)
            .FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
        if (order is null) return (false, "Order not found.");

        if (!await WebsiteHasWarehouseAsync(order.WebsiteID, ct))
            return (true, null);

        var outbound = await GetOutboundForOrderAsync(orderId, ct);
        if (outbound is { Status: (byte)StockDocumentStatus.Posted })
            return (true, null);

        var fromWh = outbound?.FromWarehouseID ?? await DefaultWarehouseId(order.WebsiteID, ct);
        var physical = BuildPhysicalLines(order);
        foreach (var line in physical)
        {
            var available = await _warehouses.GetAvailableAsync(fromWh, line.ProductVariantID, ct);
            // Reserved for this order still counts as available for this ship.
            var reservedHere = await _context.WarehouseStocks.AsNoTracking()
                .Where(s => s.WarehouseID == fromWh && s.ProductVariantID == line.ProductVariantID)
                .Select(s => s.QuantityReserved)
                .FirstOrDefaultAsync(ct);
            // Available already excludes reserved; units reserved for open orders including this one
            // sit in QuantityReserved. On-hand must cover required qty.
            var onHand = available + reservedHere;
            if (onHand < line.Quantity)
            {
                var sku = await _context.ProductVariants.AsNoTracking()
                    .Where(v => v.ProductVariantID == line.ProductVariantID)
                    .Select(v => v.Sku)
                    .FirstOrDefaultAsync(ct) ?? line.ProductVariantID.ToString();
                return (false,
                    $"Insufficient warehouse stock for {sku}: need {line.Quantity}, on hand {onHand}. Prepare stock or refund the customer.");
            }
        }
        return (true, null);
    }

    public async Task<(bool Success, string? Error, StockDocument? Doc)> EnsureReturnForRefundAsync(
        int orderId, int paymentRefundId, int? memberId, CancellationToken ct = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems).ThenInclude(i => i.ProductVariant).ThenInclude(v => v!.Product)
            .FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
        if (order is null) return (false, "Order not found.", null);

        var existingByRefund = await _context.StockDocuments
            .Include(d => d.StockDocumentLines)
            .FirstOrDefaultAsync(d =>
                d.PaymentRefundID == paymentRefundId
                && d.DocumentType == (byte)StockDocumentType.Return
                && d.Status != (byte)StockDocumentStatus.Cancelled
                && d.Status != (byte)StockDocumentStatus.Rejected, ct);
        if (existingByRefund is not null)
            return (true, null, existingByRefund);

        // Only create return stock when goods left the warehouse or inventory (post-pay fulfill).
        var outbound = await GetOutboundForOrderAsync(orderId, ct);
        var stockLeftWarehouse = outbound is { Status: (byte)StockDocumentStatus.Posted };
        var nonWmsFulfill =
            !await WebsiteHasWarehouseAsync(order.WebsiteID, ct)
            && order.Status is (byte)OrderStatus.Shipped or (byte)OrderStatus.Completed or (byte)OrderStatus.Refunded
                or (byte)OrderStatus.Processing or (byte)OrderStatus.Paid
            && order.ShippingStatus is (byte)OrderShippingStatus.Shipped
                or (byte)OrderShippingStatus.InTransit
                or (byte)OrderShippingStatus.Delivered
                or (byte)OrderShippingStatus.Returned;

        // After paid on non-WMS, inventory already left on payment — still create return for restock path.
        var nonWmsPaid = !await WebsiteHasWarehouseAsync(order.WebsiteID, ct)
            && order.PaidAt is not null;

        if (!stockLeftWarehouse && !nonWmsFulfill && !nonWmsPaid)
        {
            // Goods never left (outbound cancelled on pre-ship refund) — nothing to restock.
            return (true, null, null);
        }

        if (stockLeftWarehouse == false && nonWmsPaid && outbound is not null
            && outbound.Status is not (byte)StockDocumentStatus.Posted)
        {
            // WMS site but outbound never posted — reservation released / cancel path.
            if (await WebsiteHasWarehouseAsync(order.WebsiteID, ct))
                return (true, null, null);
        }

        var physical = BuildPhysicalLines(order);
        if (physical.Count == 0)
            return (true, null, null);

        // Ensure WMS warehouse exists for return receive when site already has warehouses.
        int? toWh = null;
        if (await WebsiteHasWarehouseAsync(order.WebsiteID, ct))
            toWh = await DefaultWarehouseId(order.WebsiteID, ct);
        else
        {
            // Non-WMS: still create Return doc so QC exists; posting will create default warehouse.
            await _warehouses.EnsureDefaultAsync(order.WebsiteID, ct);
            toWh = await DefaultWarehouseId(order.WebsiteID, ct);
        }

        var seq = await _context.StockDocuments.CountAsync(d => d.WebsiteID == order.WebsiteID, ct) + 1;
        var doc = new StockDocument
        {
            WebsiteID = order.WebsiteID,
            DocumentNumber = $"RET-{order.OrderNumber}",
            DocumentType = (byte)StockDocumentType.Return,
            Status = (byte)StockDocumentStatus.Submitted,
            ToWarehouseID = toWh,
            OrderID = order.OrderID,
            PaymentRefundID = paymentRefundId,
            Note = $"Auto return from refund #{paymentRefundId} / order {order.OrderNumber}",
            RequestedByMemberID = memberId,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };
        if (await _context.StockDocuments.AnyAsync(d => d.WebsiteID == order.WebsiteID && d.DocumentNumber == doc.DocumentNumber, ct))
            doc.DocumentNumber = $"RET-{order.OrderNumber}-{seq:D4}";

        foreach (var l in physical)
        {
            doc.StockDocumentLines.Add(new StockDocumentLine
            {
                ProductVariantID = l.ProductVariantID,
                Quantity = l.Quantity,
                UnitCost = l.UnitCost,
                UnitCostUsd = l.UnitCost,
                Note = l.Note,
                ReturnCondition = (byte)StockItemCondition.New,
                HealthGrade = 0,
            });
        }

        _context.StockDocuments.Add(doc);
        AddHistory(doc, 0, (byte)StockDocumentStatus.Submitted,
            $"Created from refund #{paymentRefundId}", memberId);
        await _context.SaveChangesAsync(ct);
        return (true, null, doc);
    }

    public async Task<(bool Success, string? Error)> SetReturnLineConditionAsync(
        int stockDocumentLineId, StockItemCondition condition, StockHealthGrade healthGrade, int? memberId, CancellationToken ct = default)
    {
        var line = await _context.StockDocumentLines
            .Include(l => l.StockDocument)
            .FirstOrDefaultAsync(l => l.StockDocumentLineID == stockDocumentLineId, ct);
        if (line is null) return (false, "Line not found.");
        if (line.StockDocument.DocumentType != (byte)StockDocumentType.Return)
            return (false, "Only return document lines have condition/QC.");
        if (line.StockDocument.Status == (byte)StockDocumentStatus.Posted)
            return (false, "Cannot change condition after post. Create a new adjustment/return if needed.");
        if (condition is StockItemCondition.None)
            return (false, "Choose a product condition.");

        if (condition is not (StockItemCondition.New or StockItemCondition.Defective)
            && healthGrade is StockHealthGrade.None)
            return (false, "Health grade is required for non-new items (LikeNew, OpenBox, Display, Used).");

        if (condition is StockItemCondition.New)
            healthGrade = StockHealthGrade.None;

        line.ReturnCondition = (byte)condition;
        line.HealthGrade = (byte)healthGrade;
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<IReadOnlyList<WarehousePickTaskDto>> GetPickQueueAsync(
        int websiteId, byte? statusFilter, CancellationToken ct = default)
    {
        var q = _context.StockDocuments.AsNoTracking()
            .Include(d => d.FromWarehouse)
            .Include(d => d.Order)
            .Include(d => d.StockDocumentLines)
            .Where(d => d.WebsiteID == websiteId
                && d.DocumentType == (byte)StockDocumentType.Outbound
                && (d.Status == (byte)StockDocumentStatus.Submitted
                    || d.Status == (byte)StockDocumentStatus.Approved));

        if (statusFilter is byte s)
            q = q.Where(d => d.Status == s);

        var rows = await q.OrderBy(d => d.SubmittedAt ?? d.CreatedAt).Take(200).ToListAsync(ct);
        return rows.Select(d => new WarehousePickTaskDto
        {
            StockDocumentID = d.StockDocumentID,
            DocumentNumber = d.DocumentNumber,
            Status = d.Status,
            OrderID = d.OrderID,
            OrderNumber = d.Order?.OrderNumber,
            LineCount = d.StockDocumentLines.Count,
            TotalQuantity = d.StockDocumentLines.Sum(l => l.Quantity),
            CreatedAt = d.CreatedAt,
            SubmittedAt = d.SubmittedAt,
            FromWarehouseName = d.FromWarehouse?.Name,
            Note = d.Note,
        }).ToList();
    }

    private static List<StockDocumentLineRequest> BuildPhysicalLines(Order order) =>
        order.OrderItems
            .Where(i => i.ProductVariantID is int)
            .Where(i =>
            {
                var p = i.ProductVariant?.Product;
                if (p is null) return true;
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
