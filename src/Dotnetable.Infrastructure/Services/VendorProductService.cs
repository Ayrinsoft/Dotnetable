using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class VendorProductService : IVendorProductService
{
    private readonly AppDbContext _context;

    public VendorProductService(AppDbContext context) => _context = context;

    public async Task<PagedResult<VendorProductListItemDto>> GetPagedAsync(int vendorId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.VendorProducts.AsNoTracking()
            .Where(vp => vp.VendorID == vendorId)
            .Include(vp => vp.Vendor)
            .Include(vp => vp.ProductVariant).ThenInclude(v => v.Product)
            .AsQueryable();

        if (query.GetSearch("Sku") is string sku)
            q = q.Where(vp => vp.ProductVariant.Sku.Contains(sku));
        if (query.GetSearch("ProductTitle") is string title)
            q = q.Where(vp => vp.ProductVariant.Product.Title.Contains(title)
                              || vp.ProductVariant.Title.Contains(title));
        if (query.GetSearch("VariantTitle") is string vTitle)
            q = q.Where(vp => vp.ProductVariant.Title.Contains(vTitle));
        if (query.GetSearch(nameof(VendorProduct.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(vp => vp.IsActive == isActive);

        // Single free-text box from list page (key "q") matches product, variant title, or SKU.
        if (query.GetSearch("q") is string free && !string.IsNullOrWhiteSpace(free))
        {
            var s = free.Trim();
            q = q.Where(vp =>
                vp.ProductVariant.Product.Title.Contains(s)
                || vp.ProductVariant.Title.Contains(s)
                || vp.ProductVariant.Sku.Contains(s));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(vp => vp.ProductVariant.Product.Title)
            .ThenBy(vp => vp.ProductVariant.Title)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<VendorProductListItemDto>
        {
            TotalCount = total,
            Items = items.Select(MapListItem).ToList(),
        };
    }

    public async Task<List<VendorProductListItemDto>> GetByProductIdAsync(int productId, CancellationToken ct = default)
    {
        var items = await _context.VendorProducts.AsNoTracking()
            .Where(vp => vp.ProductVariant.ProductID == productId)
            .Include(vp => vp.Vendor)
            .Include(vp => vp.ProductVariant).ThenInclude(v => v.Product)
            .OrderBy(vp => vp.Vendor.Name)
            .ThenBy(vp => vp.ProductVariant.Title)
            .ThenBy(vp => vp.ProductVariant.Sku)
            .ToListAsync(ct);

        return items.Select(MapListItem).ToList();
    }

    private static VendorProductListItemDto MapListItem(VendorProduct vp) => new()
    {
        VendorProductID = vp.VendorProductID,
        VendorID = vp.VendorID,
        VendorName = vp.Vendor?.Name ?? string.Empty,
        VendorType = vp.Vendor?.VendorType ?? 0,
        ProductVariantID = vp.ProductVariantID,
        ProductID = vp.ProductVariant.ProductID,
        ProductTitle = vp.ProductVariant.Product.Title,
        VariantTitle = vp.ProductVariant.Title,
        Sku = vp.ProductVariant.Sku,
        ReferencePrice = vp.ReferencePrice > 0 ? vp.ReferencePrice : vp.ReferencePriceUsd,
        ReferencePriceUsd = vp.ReferencePriceUsd,
        OverridePriceLocal = vp.OverridePriceLocal,
        OverridePrice = vp.OverridePrice,
        StockQuantity = vp.StockQuantity,
        QuantityReserved = vp.QuantityReserved,
        AvailableQuantity = IVendorProductService.Available(vp),
        DeliveryDays = vp.DeliveryDays,
        IsActive = vp.IsActive,
    };

    public async Task<List<VendorVariantPickDto>> SearchEligibleVariantsAsync(
        int vendorId, string? search, int take = 25, bool includeAlreadyListed = false, CancellationToken ct = default)
    {
        var vendor = await _context.Vendors.AsNoTracking()
            .FirstOrDefaultAsync(v => v.VendorID == vendorId, ct);
        if (vendor is null) return new List<VendorVariantPickDto>();

        // Site vendors list products owned by the linked website; others list host-website products.
        var catalogWebsiteId = vendor.VendorType == (byte)VendorType.Site && vendor.LinkedWebsiteID is int lid
            ? lid
            : vendor.WebsiteID;

        var listedIds = await _context.VendorProducts.AsNoTracking()
            .Where(vp => vp.VendorID == vendorId)
            .Select(vp => vp.ProductVariantID)
            .ToListAsync(ct);
        var listed = listedIds.ToHashSet();

        var q = _context.ProductVariants.AsNoTracking()
            .Where(v => v.IsActive
                        && v.Product.IsActive
                        && v.Product.WebsiteID == catalogWebsiteId);

        // Member sellers only pick variants from products they created (their catalog).
        if (vendor.VendorType == (byte)VendorType.Member && vendor.MemberID is int ownerId)
            q = q.Where(v => v.Product.CreatedByMemberID == ownerId);

        if (!includeAlreadyListed && listed.Count > 0)
            q = q.Where(v => !listed.Contains(v.ProductVariantID));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(v =>
                v.Sku.Contains(s)
                || v.Title.Contains(s)
                || v.Product.Title.Contains(s)
                || v.Product.Slug.Contains(s));
        }

        take = Math.Clamp(take, 1, 50);
        var rows = await q
            .OrderBy(v => v.Product.Title)
            .ThenBy(v => v.Title)
            .ThenBy(v => v.Sku)
            .Take(take)
            .Select(v => new
            {
                v.ProductVariantID,
                v.ProductID,
                ProductTitle = v.Product.Title,
                VariantTitle = v.Title,
                v.Sku,
                v.ReferencePrice,
                v.ReferencePriceUsd,
            })
            .ToListAsync(ct);

        return rows.Select(v => new VendorVariantPickDto
        {
            ProductVariantID = v.ProductVariantID,
            ProductID = v.ProductID,
            ProductTitle = v.ProductTitle,
            VariantTitle = v.VariantTitle ?? string.Empty,
            Sku = v.Sku,
            ReferencePrice = v.ReferencePrice > 0 ? v.ReferencePrice : v.ReferencePriceUsd,
            ReferencePriceUsd = v.ReferencePriceUsd,
            AlreadyListed = listed.Contains(v.ProductVariantID),
        }).ToList();
    }

    public async Task<VendorProduct?> GetByIdAsync(int vendorProductId, CancellationToken ct = default) =>
        await _context.VendorProducts
            .Include(vp => vp.Vendor)
            .Include(vp => vp.ProductVariant).ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(vp => vp.VendorProductID == vendorProductId, ct);

    public async Task<VendorProduct?> FindAsync(int vendorId, int productVariantId, CancellationToken ct = default) =>
        await _context.VendorProducts
            .FirstOrDefaultAsync(vp => vp.VendorID == vendorId && vp.ProductVariantID == productVariantId, ct);

    public async Task<(bool Success, string? Error, VendorProduct? Item)> UpsertAsync(
        VendorProduct model, int? actingMemberId = null, CancellationToken ct = default)
    {
        var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.VendorID == model.VendorID, ct);
        if (vendor is null) return (false, "Vendor not found.", null);

        if (actingMemberId is int mid)
        {
            if (vendor.VendorType != (byte)VendorType.Member || vendor.MemberID != mid)
                return (false, "You can only manage listings for your own vendor account.", null);
        }

        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.ProductVariantID == model.ProductVariantID, ct);
        if (variant is null) return (false, "Product variant not found.", null);

        var ownershipError = ValidateOwnership(vendor, variant);
        if (ownershipError is not null) return (false, ownershipError, null);

        VendorProduct entity;
        if (model.VendorProductID > 0)
        {
            entity = await _context.VendorProducts.FirstOrDefaultAsync(vp => vp.VendorProductID == model.VendorProductID, ct)
                     ?? throw new InvalidOperationException("Vendor product not found.");
            if (entity.VendorID != model.VendorID)
                return (false, "Vendor product mismatch.", null);
        }
        else
        {
            var existing = await FindAsync(model.VendorID, model.ProductVariantID, ct);
            if (existing is not null)
            {
                entity = existing;
            }
            else
            {
                entity = new VendorProduct
                {
                    WebsiteID = vendor.WebsiteID,
                    VendorID = vendor.VendorID,
                    ProductVariantID = model.ProductVariantID,
                };
                _context.VendorProducts.Add(entity);
            }
        }

        // Site-currency listing price is authority; USD dual kept for conversion bridge.
        entity.ReferencePrice = model.ReferencePrice > 0
            ? model.ReferencePrice
            : (variant.ReferencePrice > 0 ? variant.ReferencePrice : model.ReferencePriceUsd);
        entity.OverridePriceLocal = model.OverridePriceLocal;
        entity.ReferencePriceUsd = model.ReferencePriceUsd > 0
            ? model.ReferencePriceUsd
            : (variant.ReferencePriceUsd > 0 ? variant.ReferencePriceUsd : entity.ReferencePrice);
        entity.OverridePrice = model.OverridePrice;
        var desiredStock = Math.Max(0, model.StockQuantity);
        if (desiredStock < entity.QuantityReserved)
            return (false, $"Stock ({desiredStock}) cannot be below reserved quantity ({entity.QuantityReserved}).", null);
        entity.StockQuantity = desiredStock;
        entity.DeliveryDays = model.DeliveryDays < 0 ? 0 : model.DeliveryDays;
        entity.IsActive = model.IsActive;
        entity.WebsiteID = vendor.WebsiteID;

        await _context.SaveChangesAsync(ct);
        // Inventory total for the host variant = sum of store listing stocks.
        await SyncInventoryOnHandFromListingsAsync(entity.WebsiteID, entity.ProductVariantID, ct);
        return (true, null, entity);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int vendorProductId, int? actingMemberId = null, CancellationToken ct = default)
    {
        var entity = await _context.VendorProducts
            .Include(vp => vp.Vendor)
            .FirstOrDefaultAsync(vp => vp.VendorProductID == vendorProductId, ct);
        if (entity is null) return (true, null);

        if (actingMemberId is int mid)
        {
            if (entity.Vendor.VendorType != (byte)VendorType.Member || entity.Vendor.MemberID != mid)
                return (false, "You can only manage listings for your own vendor account.");
        }

        if (entity.QuantityReserved > 0)
            return (false, "Cannot delete a listing with reserved stock from open orders.");

        var websiteId = entity.WebsiteID;
        var variantId = entity.ProductVariantID;
        _context.VendorProducts.Remove(entity);
        await _context.SaveChangesAsync(ct);
        await SyncInventoryOnHandFromListingsAsync(websiteId, variantId, ct);
        return (true, null);
    }

    public async Task<VendorProduct?> EnsureListingForVariantAsync(
        int hostWebsiteId, int vendorId, int productVariantId, CancellationToken ct = default)
    {
        var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.VendorID == vendorId && v.WebsiteID == hostWebsiteId, ct);
        if (vendor is null || !vendor.IsActive) return null;

        var existing = await FindAsync(vendorId, productVariantId, ct);
        if (existing is not null) return existing;

        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.ProductVariantID == productVariantId, ct);
        if (variant is null) return null;
        if (ValidateOwnership(vendor, variant) is not null) return null;

        // Seed from source-site store listings only (never raw warehouse). Zero ⇒ not sellable until stock is set.
        var sourceStock = await _context.VendorProducts.AsNoTracking()
            .Where(vp => vp.ProductVariantID == productVariantId
                         && vp.WebsiteID == variant.WebsiteID
                         && vp.IsActive)
            .Select(vp => vp.StockQuantity - vp.QuantityReserved)
            .SumAsync(ct);

        var listing = new VendorProduct
        {
            WebsiteID = hostWebsiteId,
            VendorID = vendorId,
            ProductVariantID = productVariantId,
            // Per-seller listing price starts from the catalog variant; sellers can override later.
            ReferencePrice = variant.ReferencePrice > 0 ? variant.ReferencePrice : variant.ReferencePriceUsd,
            ReferencePriceUsd = variant.ReferencePriceUsd > 0 ? variant.ReferencePriceUsd : variant.ReferencePrice,
            OverridePriceLocal = null,
            OverridePrice = null,
            StockQuantity = Math.Max(0, sourceStock),
            QuantityReserved = 0,
            DeliveryDays = 1,
            IsActive = true,
        };
        _context.VendorProducts.Add(listing);
        await _context.SaveChangesAsync(ct);
        await SyncInventoryOnHandFromListingsAsync(hostWebsiteId, productVariantId, ct);
        return listing;
    }

    public async Task<List<VendorProduct>> GetActiveByVendorAsync(int vendorId, CancellationToken ct = default) =>
        await _context.VendorProducts.AsNoTracking()
            .Where(vp => vp.VendorID == vendorId && vp.IsActive)
            .Include(vp => vp.ProductVariant).ThenInclude(v => v.Product)
            .ToListAsync(ct);

    public async Task<bool> ReserveAsync(int vendorProductId, int qty, CancellationToken ct = default)
    {
        if (qty <= 0) return true;
        var item = await _context.VendorProducts.FirstOrDefaultAsync(vp => vp.VendorProductID == vendorProductId, ct);
        if (item is null || !item.IsActive) return false;
        if (IVendorProductService.Available(item) < qty) return false;

        item.QuantityReserved += qty;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task ReleaseReservationAsync(int vendorProductId, int qty, CancellationToken ct = default)
    {
        if (qty <= 0) return;
        var item = await _context.VendorProducts.FirstOrDefaultAsync(vp => vp.VendorProductID == vendorProductId, ct);
        if (item is null) return;

        item.QuantityReserved = Math.Max(0, item.QuantityReserved - qty);
        await _context.SaveChangesAsync(ct);
    }

    public async Task CommitSaleAsync(int vendorProductId, int qty, CancellationToken ct = default)
    {
        if (qty <= 0) return;
        var item = await _context.VendorProducts.FirstOrDefaultAsync(vp => vp.VendorProductID == vendorProductId, ct);
        if (item is null) return;

        item.StockQuantity = Math.Max(0, item.StockQuantity - qty);
        item.QuantityReserved = Math.Max(0, item.QuantityReserved - qty);
        await _context.SaveChangesAsync(ct);
        // Inventory on-hand is reduced via DecrementOnFulfill; re-sync after both complete in OrderService.
    }

    public async Task SyncInventoryOnHandFromListingsAsync(int websiteId, int productVariantId, CancellationToken ct = default)
    {
        var listings = await _context.VendorProducts
            .Where(vp => vp.WebsiteID == websiteId && vp.ProductVariantID == productVariantId)
            .ToListAsync(ct);

        // Source of truth for total inventory: sum of store listing on-hand quantities (0 when no stores).
        var sumOnHand = listings.Sum(vp => Math.Max(0, vp.StockQuantity));

        var inv = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.WebsiteID == websiteId && i.ProductVariantID == productVariantId, ct);
        if (listings.Count == 0)
        {
            if (inv is null) return;
            // No store listings ⇒ nothing sellable; keep on-hand at reserved floor only.
            inv.QuantityOnHand = inv.QuantityReserved;
            await _context.SaveChangesAsync(ct);
            return;
        }

        if (inv is null)
        {
            inv = new InventoryItem
            {
                WebsiteID = websiteId,
                ProductVariantID = productVariantId,
                QuantityOnHand = sumOnHand,
                QuantityReserved = 0,
                ReorderLevel = 0,
                AvgCost = 0,
                AvgCostUsd = 0,
                RowVersion = new byte[8],
            };
            _context.InventoryItems.Add(inv);
        }
        else
        {
            // Never drop on-hand below open reservations held at inventory level.
            inv.QuantityOnHand = Math.Max(sumOnHand, inv.QuantityReserved);
        }

        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Site vendors may only list products owned by the linked website (Product.WebsiteID == LinkedWebsiteID).
    /// This blocks re-sharing goods the linked site itself obtained from other sites.
    /// Member vendors may only list products on the host website (and preferably created by them).
    /// Display vendors may list any host product.
    /// </summary>
    private static string? ValidateOwnership(Vendor vendor, ProductVariant variant)
    {
        return (VendorType)vendor.VendorType switch
        {
            VendorType.Site when vendor.LinkedWebsiteID is int lid =>
                variant.Product.WebsiteID == lid
                    ? null
                    : "Only products owned by the linked website can be listed (borrowed products cannot be re-shared).",
            VendorType.Member =>
                variant.Product.WebsiteID != vendor.WebsiteID
                    ? "Member vendors can only list products on their host website."
                    : vendor.MemberID is int mid && variant.Product.CreatedByMemberID != mid
                        ? "Member vendors can only list products they created."
                        : null,
            _ =>
                variant.Product.WebsiteID == vendor.WebsiteID
                    ? null
                    : "Display vendors can only list products on the host website.",
        };
    }
}
