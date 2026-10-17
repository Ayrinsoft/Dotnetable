using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class SlowSellerService : ISlowSellerService
{
    private static readonly byte[] SoldStatuses =
    [
        (byte)OrderStatus.Paid,
        (byte)OrderStatus.Processing,
        (byte)OrderStatus.Shipped,
        (byte)OrderStatus.Completed,
    ];

    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public SlowSellerService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<SlowSellerReport> GetAsync(int websiteId, int maxUnitsSold, string? search, GridQuery query, CancellationToken ct = default)
    {
        maxUnitsSold = Math.Clamp(maxUnitsSold, 0, 1_000_000);
        var to = DateTime.UtcNow;
        var from = to.AddDays(-ISlowSellerService.WindowDays);

        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var currency = await db.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .Select(w => w.DefaultCurrencyCode)
            .FirstOrDefaultAsync(ct);

        var products = db.Products.AsNoTracking()
            .Where(p => p.WebsiteID == websiteId && p.IsActive && p.Status == ProductService.PublishedStatus && !p.IsCatalogOnly);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            products = products.Where(p => p.Title.Contains(term) || p.Slug.Contains(term));
        }

        var filtered = products.Select(p => new
        {
            p.ProductID,
            p.Title,
            p.Slug,
            Units = db.OrderItems
                .Where(i => i.ProductVariant != null
                    && i.ProductVariant.ProductID == p.ProductID
                    && i.Order.WebsiteID == websiteId
                    && SoldStatuses.Contains(i.Order.Status)
                    && (i.Order.PaidAt ?? i.Order.CreatedAt) >= from)
                .Sum(i => (int?)i.Quantity) ?? 0,
            Orders = db.OrderItems
                .Where(i => i.ProductVariant != null
                    && i.ProductVariant.ProductID == p.ProductID
                    && i.Order.WebsiteID == websiteId
                    && SoldStatuses.Contains(i.Order.Status)
                    && (i.Order.PaidAt ?? i.Order.CreatedAt) >= from)
                .Select(i => i.OrderID)
                .Distinct()
                .Count(),
            Price = p.ProductVariants
                .Where(v => v.IsActive)
                .OrderByDescending(v => v.IsDefault)
                .Select(v => (decimal?)v.ReferencePrice)
                .FirstOrDefault(),
            HasDiscount = p.ProductVariants.Any(v => v.IsActive && v.CompareAtPrice != null && v.CompareAtPrice > v.ReferencePrice),
            Stock = p.ProductVariants.SelectMany(v => v.InventoryItems).Sum(i => (int?)i.QuantityOnHand) ?? 0,
        }).Where(x => x.Units <= maxUnitsSold);

        var total = await filtered.CountAsync(ct);
        var items = await filtered
            .OrderBy(x => x.Units).ThenBy(x => x.Title)
            .Skip(query.Skip).Take(query.Take)
            .Select(x => new SlowSellerRow
            {
                ProductId = x.ProductID,
                Title = x.Title,
                Slug = x.Slug,
                UnitsSold = x.Units,
                OrderCount = x.Orders,
                Price = x.Price,
                HasDiscount = x.HasDiscount,
                StockOnHand = x.Stock,
            })
            .ToListAsync(ct);

        return new SlowSellerReport
        {
            FromUtc = from,
            ToUtc = to,
            CurrencyCode = currency,
            Items = items,
            TotalCount = total,
        };
    }
}
