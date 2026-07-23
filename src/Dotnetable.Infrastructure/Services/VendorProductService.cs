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
            .Include(vp => vp.ProductVariant).ThenInclude(v => v.Product)
            .AsQueryable();

        if (query.GetSearch("Sku") is string sku)
            q = q.Where(vp => vp.ProductVariant.Sku.Contains(sku));
        if (query.GetSearch("ProductTitle") is string title)
            q = q.Where(vp => vp.ProductVariant.Product.Title.Contains(title));
        if (query.GetSearch(nameof(VendorProduct.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(vp => vp.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(vp => vp.ProductVariant.Product.Title)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<VendorProductListItemDto>
        {
            TotalCount = total,
            Items = items.Select(vp => new VendorProductListItemDto
            {
                VendorProductID = vp.VendorProductID,
                VendorID = vp.VendorID,
                ProductVariantID = vp.ProductVariantID,
                ProductID = vp.ProductVariant.ProductID,
                ProductTitle = vp.ProductVariant.Product.Title,
                Sku = vp.ProductVariant.Sku,
                ReferencePriceUsd = vp.ReferencePriceUsd,
                OverridePrice = vp.OverridePrice,
                StockQuantity = vp.StockQuantity,
                DeliveryDays = vp.DeliveryDays,
                IsActive = vp.IsActive,
            }).ToList(),
        };
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

        entity.ReferencePriceUsd = model.ReferencePriceUsd > 0
            ? model.ReferencePriceUsd
            : variant.ReferencePriceUsd;
        entity.OverridePrice = model.OverridePrice;
        entity.StockQuantity = model.StockQuantity;
        entity.DeliveryDays = model.DeliveryDays < 0 ? 0 : model.DeliveryDays;
        entity.IsActive = model.IsActive;
        entity.WebsiteID = vendor.WebsiteID;

        await _context.SaveChangesAsync(ct);
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

        _context.VendorProducts.Remove(entity);
        await _context.SaveChangesAsync(ct);
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

        var sourceStock = await _context.InventoryItems.AsNoTracking()
            .Where(i => i.ProductVariantID == productVariantId && i.WebsiteID == variant.WebsiteID)
            .Select(i => i.QuantityOnHand - i.QuantityReserved)
            .FirstOrDefaultAsync(ct);

        var listing = new VendorProduct
        {
            WebsiteID = hostWebsiteId,
            VendorID = vendorId,
            ProductVariantID = productVariantId,
            ReferencePriceUsd = variant.ReferencePriceUsd,
            OverridePrice = null,
            StockQuantity = Math.Max(0, sourceStock),
            DeliveryDays = 1,
            IsActive = true,
        };
        _context.VendorProducts.Add(listing);
        await _context.SaveChangesAsync(ct);
        return listing;
    }

    public async Task<List<VendorProduct>> GetActiveByVendorAsync(int vendorId, CancellationToken ct = default) =>
        await _context.VendorProducts.AsNoTracking()
            .Where(vp => vp.VendorID == vendorId && vp.IsActive)
            .Include(vp => vp.ProductVariant).ThenInclude(v => v.Product)
            .ToListAsync(ct);

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
                variant.Product.WebsiteID == vendor.WebsiteID
                    ? null
                    : "Member vendors can only list products on their host website.",
            _ =>
                variant.Product.WebsiteID == vendor.WebsiteID
                    ? null
                    : "Display vendors can only list products on the host website.",
        };
    }
}
