using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Marketplace;

/// <summary>
/// Flattens the catalog into the lines one channel is offered: active variants of active products,
/// filtered by that channel's category rules and per-product overrides, priced and stocked. Providers
/// never touch the database — they receive this list.
/// </summary>
public sealed class MarketplaceProductSource : IMarketplaceProductSource
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public MarketplaceProductSource(IDbContextFactory<AppDbContext> contextFactory) =>
        _contextFactory = contextFactory;

    public async Task<IReadOnlyList<MarketplaceProductItem>> GetItemsAsync(int channelId,
        CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var channel = await _context.MarketplaceChannels
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.MarketplaceChannelID == channelId, ct);
        if (channel is null) return [];

        var website = await _context.Websites
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebsiteID == channel.WebsiteID, ct);
        if (website is null) return [];

        var categoryRules = await _context.MarketplaceChannelCategories
            .AsNoTracking()
            .Where(c => c.MarketplaceChannelID == channelId)
            .ToDictionaryAsync(c => c.ProductCategoryID, ct);

        var overrides = await _context.MarketplaceChannelProducts
            .AsNoTracking()
            .Where(p => p.MarketplaceChannelID == channelId)
            .ToDictionaryAsync(p => p.ProductID, ct);

        var includedCategories = categoryRules.Values
            .Where(r => r.IsIncluded)
            .Select(r => r.ProductCategoryID)
            .ToHashSet();

        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.FeaturedImageFile)
            .Include(p => p.ProductCategoryMaps).ThenInclude(m => m.ProductCategory)
            .Include(p => p.ProductVariants).ThenInclude(v => v.ImageFile)
            .Where(p => p.WebsiteID == channel.WebsiteID && p.IsActive && !p.IsCatalogOnly)
            .ToListAsync(ct);

        var variantIds = products.SelectMany(p => p.ProductVariants)
            .Where(v => v.IsActive)
            .Select(v => v.ProductVariantID)
            .ToList();

        var stock = await _context.InventoryItems
            .AsNoTracking()
            .Where(i => variantIds.Contains(i.ProductVariantID))
            .GroupBy(i => i.ProductVariantID)
            .Select(g => new { VariantId = g.Key, Available = g.Sum(i => i.QuantityOnHand - i.QuantityReserved) })
            .ToDictionaryAsync(x => x.VariantId, x => x.Available, ct);

        var baseUrl = BuildBaseUrl(website.WebsiteAddress);
        var items = new List<MarketplaceProductItem>();

        foreach (var product in products)
        {
            if (!IsOffered(product.ProductID, product.ProductCategoryMaps.Select(m => m.ProductCategoryID),
                    channel.IncludeAllProducts, includedCategories, overrides))
                continue;

            var primaryMap = product.ProductCategoryMaps.FirstOrDefault(m => m.IsPrimary)
                ?? product.ProductCategoryMaps.FirstOrDefault();
            categoryRules.TryGetValue(primaryMap?.ProductCategoryID ?? 0, out var mapping);
            overrides.TryGetValue(product.ProductID, out var productOverride);

            var galleryUrls = product.FeaturedImageFile is null ? [] : new List<string>();

            foreach (var variant in product.ProductVariants.Where(v => v.IsActive))
            {
                var available = stock.GetValueOrDefault(variant.ProductVariantID);
                var imageUrl = ImageUrl(variant.ImageFile) ?? ImageUrl(product.FeaturedImageFile);

                items.Add(new MarketplaceProductItem
                {
                    ProductID = product.ProductID,
                    ProductVariantID = variant.ProductVariantID,
                    Id = string.IsNullOrWhiteSpace(variant.Sku)
                        ? variant.ProductVariantID.ToString()
                        : variant.Sku,
                    Sku = variant.Sku,
                    Barcode = variant.Barcode,
                    Title = product.HasVariants && !variant.IsDefault
                        ? $"{product.Title} - {variant.Title}"
                        : product.Title,
                    Description = product.ShortDescription,
                    Url = $"{baseUrl}/product/{product.Slug}",
                    ImageUrl = imageUrl,
                    AdditionalImageUrls = galleryUrls,
                    Price = variant.OverridePrice ?? variant.ReferencePrice,
                    CompareAtPrice = variant.CompareAtPrice,
                    InStock = available > 0,
                    AvailableQuantity = available < 0 ? 0 : available,
                    Brand = product.Brand?.Name,
                    CategoryName = primaryMap?.ProductCategory?.Name,
                    RemoteCategoryID = mapping?.RemoteCategoryID,
                    RemoteCategoryPath = mapping?.RemoteCategoryPath,
                    RemoteProductID = productOverride?.RemoteProductID,
                });
            }
        }

        return items;
    }

    /// <summary>
    /// A per-product override always wins; otherwise the channel either takes everything or takes only
    /// products sitting in an included category.
    /// </summary>
    private static bool IsOffered(int productId, IEnumerable<int> categoryIds, bool includeAll,
        IReadOnlySet<int> includedCategories,
        IReadOnlyDictionary<int, Domain.Entities.MarketplaceChannelProduct> overrides)
    {
        if (overrides.TryGetValue(productId, out var row)) return row.IsIncluded;
        if (includeAll) return true;
        return categoryIds.Any(includedCategories.Contains);
    }

    /// <summary>Feeds want the full-size image, not the thumbnail — Google rejects images under 250px.</summary>
    private static string? ImageUrl(Domain.Entities.FileRecord? file)
    {
        if (file is null || file.IsDeleted) return null;
        var url = !string.IsNullOrWhiteSpace(file.CNDUrl) ? file.CNDUrl : file.ThumbnailCDN;
        return string.IsNullOrWhiteSpace(url) ? null : url.Trim();
    }

    /// <summary>Website addresses are stored bare (<c>shop.example.com</c>), but feeds need absolute URLs.</summary>
    private static string BuildBaseUrl(string websiteAddress)
    {
        var address = (websiteAddress ?? "").Trim().TrimEnd('/');
        if (address.Length == 0) return "";
        return address.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               address.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? address
            : $"https://{address}";
    }
}
