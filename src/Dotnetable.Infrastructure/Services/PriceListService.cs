using System.Text.RegularExpressions;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class PriceListService : IPriceListService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ICurrencyRateService _currencyRates;

    public PriceListService(IDbContextFactory<AppDbContext> contextFactory, ICurrencyRateService currencyRates)
    {
        _contextFactory = contextFactory;
        _currencyRates = currencyRates;
    }

    public async Task<List<PriceList>> GetAllAsync(int? websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.PriceLists.AsNoTracking().Include(l => l.PriceListItems).AsQueryable();
        if (websiteId is int wid)
            q = q.Where(l => l.WebsiteID == wid);
        return await q.OrderBy(l => l.SortOrder).ThenBy(l => l.Title).ToListAsync(ct);
    }

    public async Task<PagedResult<PriceList>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.PriceLists.AsNoTracking().Include(l => l.PriceListItems).AsQueryable();
        if (websiteId is int wid)
            q = q.Where(l => l.WebsiteID == wid);

        if (query.GetSearch(nameof(PriceList.Title)) is string title)
            q = q.Where(l => l.Title.Contains(title));
        if (query.GetSearch(nameof(PriceList.Slug)) is string slug)
            q = q.Where(l => l.Slug.Contains(slug));
        if (query.GetSearch(nameof(PriceList.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(l => l.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(PriceList.SortOrder))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<PriceList> { Items = items, TotalCount = total };
    }

    public async Task<PriceList?> GetAsync(int priceListId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        return await _context.PriceLists.FindAsync([priceListId], ct);
    }

    public async Task<PriceList> CreateAsync(PriceList list, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        NormalizeList(list);
        await EnsureUniqueSlugAsync(_context, list.WebsiteID, list.Slug, excludeId: null, ct);

        var now = DateTime.UtcNow;
        list.CreatedAt = now;
        list.UpdatedAt = now;
        _context.PriceLists.Add(list);
        await _context.SaveChangesAsync(ct);
        return list;
    }

    public async Task UpdateAsync(PriceList list, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.PriceLists
            .FirstOrDefaultAsync(l => l.PriceListID == list.PriceListID, ct)
            ?? throw new InvalidOperationException("Price list not found.");

        NormalizeList(list);
        await EnsureUniqueSlugAsync(_context, existing.WebsiteID, list.Slug, existing.PriceListID, ct);

        existing.Title = list.Title;
        existing.Slug = list.Slug;
        existing.Description = list.Description;
        existing.Notes = list.Notes;
        existing.Source = list.Source;
        existing.Pricing = list.Pricing;
        existing.ProductCategoryID = list.ProductCategoryID;
        existing.BrandID = list.BrandID;
        existing.SortOrder = list.SortOrder;
        existing.IsActive = list.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int priceListId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var list = await _context.PriceLists
            .Include(l => l.PriceListItems)
            .FirstOrDefaultAsync(l => l.PriceListID == priceListId, ct);
        if (list is null) return;

        _context.PriceListItems.RemoveRange(list.PriceListItems);
        _context.PriceLists.Remove(list);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<PriceListItem>> GetItemsAsync(int priceListId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.PriceListItems.AsNoTracking()
            .Where(i => i.PriceListID == priceListId)
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.GroupName)
            .ThenBy(i => i.Title)
            .ToListAsync(ct);
    }

    public async Task<PriceListItem?> GetItemAsync(int itemId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        return await _context.PriceListItems.FindAsync([itemId], ct);
    }

    public async Task<PriceListItem> CreateItemAsync(PriceListItem item, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        NormalizeItem(item);
        item.UpdatedAt = DateTime.UtcNow;
        _context.PriceListItems.Add(item);

        var list = await _context.PriceLists.FirstOrDefaultAsync(l => l.PriceListID == item.PriceListID, ct);
        if (list is not null) list.UpdatedAt = item.UpdatedAt;

        await _context.SaveChangesAsync(ct);
        return item;
    }

    public async Task UpdateItemAsync(PriceListItem item, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.PriceListItems
            .FirstOrDefaultAsync(i => i.PriceListItemID == item.PriceListItemID, ct)
            ?? throw new InvalidOperationException("Price list item not found.");

        NormalizeItem(item);
        existing.Title = item.Title;
        existing.GroupName = item.GroupName;
        existing.Specification = item.Specification;
        existing.Unit = item.Unit;
        existing.Sku = item.Sku;
        existing.BasePriceUsd = item.BasePriceUsd;
        existing.LinkToUsd = item.LinkToUsd;
        existing.FixedPrice = item.FixedPrice;
        existing.Notes = item.Notes;
        existing.SortOrder = item.SortOrder;
        existing.IsActive = item.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        var list = await _context.PriceLists.FirstOrDefaultAsync(l => l.PriceListID == existing.PriceListID, ct);
        if (list is not null) list.UpdatedAt = existing.UpdatedAt;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteItemAsync(int itemId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var item = await _context.PriceListItems.FirstOrDefaultAsync(i => i.PriceListItemID == itemId, ct);
        if (item is null) return;

        var listId = item.PriceListID;
        _context.PriceListItems.Remove(item);

        var list = await _context.PriceLists.FirstOrDefaultAsync(l => l.PriceListID == listId, ct);
        if (list is not null) list.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
    }

    public async Task<PriceListRateSnapshot> GetRateSnapshotAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        return await LoadRateAsync(_context, websiteId, ct);
    }

    public async Task<PriceListRateSnapshot?> UpdateUsdRateAsync(int websiteId, decimal usdToCurrency, CancellationToken ct = default)
    {
        var updated = await _currencyRates.UpdateDefaultUsdToCurrencyAsync(websiteId, usdToCurrency, ct);
        if (updated is null) return null;
        return await GetRateSnapshotAsync(websiteId, ct);
    }

    public async Task<IReadOnlyList<PriceListSummaryDto>> GetPublishedAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var rate = await LoadRateAsync(_context, websiteId, ct);
        var lists = await _context.PriceLists.AsNoTracking()
            .Include(l => l.PriceListItems)
            .Where(l => l.WebsiteID == websiteId && l.IsActive)
            .OrderBy(l => l.SortOrder)
            .ThenBy(l => l.Title)
            .ToListAsync(ct);

        var result = new List<PriceListSummaryDto>(lists.Count);
        foreach (var l in lists)
        {
            var detail = await ProjectPublishedAsync(_context, l, rate, ct);
            result.Add(new PriceListSummaryDto
            {
                PriceListID = l.PriceListID,
                Title = l.Title,
                Slug = l.Slug,
                Description = l.Description,
                ItemCount = detail.Groups.Sum(g => g.Items.Count),
                LastUpdatedAt = detail.LastUpdatedAt,
                CurrencyCode = detail.CurrencyCode,
                Source = detail.Source,
                FollowsUsd = detail.FollowsUsd,
            });
        }

        return result;
    }

    public async Task<PriceListDetailDto?> GetPublishedBySlugAsync(int websiteId, string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;

        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var list = await _context.PriceLists.AsNoTracking()
            .Include(l => l.PriceListItems)
            .FirstOrDefaultAsync(l => l.WebsiteID == websiteId && l.IsActive && l.Slug == slug, ct);
        if (list is null) return null;

        var rate = await LoadRateAsync(_context, websiteId, ct);
        return await ProjectPublishedAsync(_context, list, rate, ct);
    }

    private async Task<PriceListDetailDto> ProjectPublishedAsync(
        AppDbContext _context, PriceList list, PriceListRateSnapshot rate, CancellationToken ct)
    {
        var source = (PriceListSource)list.Source;
        var followsUsd = source == PriceListSource.Custom && list.Pricing == (byte)PriceListPricing.LinkedToUsd;

        List<PriceListItemDto> items;
        DateTime lastUpdated;
        if (source == PriceListSource.CatalogProducts)
        {
            (items, lastUpdated) = await LoadCatalogRowsAsync(_context, list, rate, ct);
            if (list.UpdatedAt > lastUpdated) lastUpdated = list.UpdatedAt;
        }
        else
        {
            items = list.PriceListItems
                .Where(i => i.IsActive)
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.GroupName)
                .ThenBy(i => i.Title)
                .Select(i => MapCustomItem(i, rate, followsUsd))
                .ToList();
            lastUpdated = list.UpdatedAt;
            foreach (var row in list.PriceListItems)
            {
                if (row.UpdatedAt > lastUpdated) lastUpdated = row.UpdatedAt;
            }

            if (followsUsd && items.Any(i => i.LinkToUsd) && rate.LastUpdate is DateTime fx && fx > lastUpdated)
                lastUpdated = fx;
        }

        var groups = items
            .GroupBy(i => string.IsNullOrWhiteSpace(i.GroupName) ? "" : i.GroupName.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => new PriceListGroupDto { Name = g.Key, Items = g.ToList() })
            .ToList();

        return new PriceListDetailDto
        {
            PriceListID = list.PriceListID,
            Title = list.Title,
            Slug = list.Slug,
            Description = list.Description,
            LastUpdatedAt = lastUpdated,
            CurrencyCode = rate.CurrencyCode,
            DecimalDigits = rate.DecimalDigits,
            Source = source,
            FollowsUsd = followsUsd,
            UsdToCurrency = followsUsd ? rate.UsdToCurrency : 0m,
            ExchangeRateLastUpdate = followsUsd ? rate.LastUpdate : null,
            Groups = groups,
        };
    }

    private static PriceListItemDto MapCustomItem(PriceListItem item, PriceListRateSnapshot rate, bool listFollowsUsd)
    {
        var linked = listFollowsUsd && item.LinkToUsd;
        decimal amountUsd;
        decimal amount;
        if (linked)
        {
            amountUsd = item.BasePriceUsd;
            amount = Math.Round(item.BasePriceUsd * rate.UsdToCurrency, rate.DecimalDigits, MidpointRounding.AwayFromZero);
        }
        else
        {
            amount = Math.Round(item.FixedPrice ?? 0m, rate.DecimalDigits, MidpointRounding.AwayFromZero);
            amountUsd = rate.UsdToCurrency > 0 ? amount / rate.UsdToCurrency : amount;
        }

        return new PriceListItemDto
        {
            PriceListItemID = item.PriceListItemID,
            Title = item.Title,
            GroupName = item.GroupName,
            Specification = item.Specification,
            Unit = item.Unit,
            Sku = item.Sku,
            LinkToUsd = linked,
            Price = new MoneyDto
            {
                Amount = amount,
                AmountUsd = amountUsd,
                CurrencyCode = rate.CurrencyCode,
            },
        };
    }

    private async Task<(List<PriceListItemDto> Items, DateTime LastUpdated)> LoadCatalogRowsAsync(
        AppDbContext _context, PriceList list, PriceListRateSnapshot rate, CancellationToken ct)
    {
        var q = _context.Products.AsNoTracking()
            .Where(p => p.WebsiteID == list.WebsiteID && p.IsActive && p.Status == ProductService.PublishedStatus)
            .Include(p => p.ProductVariants)
            .Include(p => p.Brand)
            .Include(p => p.ProductCategoryMaps).ThenInclude(m => m.ProductCategory)
            .AsQueryable();

        if (list.BrandID is int brandId)
            q = q.Where(p => p.BrandID == brandId);

        if (list.ProductCategoryID is int categoryId)
        {
            var categoryIds = await CollectCategoryIdsAsync(_context, list.WebsiteID, categoryId, ct);
            q = q.Where(p => p.ProductCategoryMaps.Any(m => categoryIds.Contains(m.ProductCategoryID)));
        }

        var products = await q
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Title)
            .ToListAsync(ct);

        var items = new List<PriceListItemDto>(products.Count);
        var last = list.UpdatedAt;
        foreach (var product in products)
        {
            if (product.UpdatedAt > last) last = product.UpdatedAt;
            var variant = product.ProductVariants
                .Where(v => v.IsActive)
                .OrderByDescending(v => v.IsDefault)
                .ThenBy(v => v.ProductVariantID)
                .FirstOrDefault();
            if (variant is null) continue;

            var primary = product.ProductCategoryMaps.FirstOrDefault(m => m.IsPrimary)
                          ?? product.ProductCategoryMaps.FirstOrDefault();
            var group = primary?.ProductCategory?.Name
                        ?? product.Brand?.Name
                        ?? "";
            var spec = !string.IsNullOrWhiteSpace(variant.Title) && !variant.IsDefault
                ? variant.Title
                : TrimOrNull(product.ShortDescription, 300);

            var amount = Math.Round(variant.ReferencePrice, rate.DecimalDigits, MidpointRounding.AwayFromZero);
            items.Add(new PriceListItemDto
            {
                PriceListItemID = 0,
                Title = product.Title,
                GroupName = group,
                Specification = spec,
                Sku = variant.Sku,
                LinkToUsd = false,
                ProductSlug = product.Slug,
                Price = new MoneyDto
                {
                    Amount = amount,
                    AmountUsd = variant.ReferencePriceUsd,
                    CurrencyCode = rate.CurrencyCode,
                },
            });
        }

        return (items, last);
    }

    private static async Task<HashSet<int>> CollectCategoryIdsAsync(
        AppDbContext _context, int websiteId, int rootId, CancellationToken ct)
    {
        var all = await _context.ProductCategories.AsNoTracking()
            .Where(c => c.WebsiteID == websiteId)
            .Select(c => new { c.ProductCategoryID, c.ParentCategoryID })
            .ToListAsync(ct);

        var children = all
            .Where(c => c.ParentCategoryID is not null)
            .GroupBy(c => c.ParentCategoryID!.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ProductCategoryID).ToList());

        var ids = new HashSet<int> { rootId };
        var stack = new Stack<int>();
        stack.Push(rootId);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!children.TryGetValue(current, out var kids)) continue;
            foreach (var kid in kids)
            {
                if (ids.Add(kid))
                    stack.Push(kid);
            }
        }

        return ids;
    }

    private static async Task<PriceListRateSnapshot> LoadRateAsync(AppDbContext _context, int websiteId, CancellationToken ct)
    {
        var rate = await _context.CurrencyRates.AsNoTracking()
            .Include(r => r.CurrencyCodeNavigation)
            .Where(r => r.WebsiteID == websiteId)
            .OrderByDescending(r => r.IsDefault)
            .ThenBy(r => r.CurrencyRateID)
            .FirstOrDefaultAsync(ct);

        if (rate is null)
        {
            var website = await _context.Websites.AsNoTracking()
                .FirstOrDefaultAsync(w => w.WebsiteID == websiteId, ct);
            var code = website?.DefaultCurrencyCode ?? "USD";
            var digits = await _context.Currencies.AsNoTracking()
                .Where(c => c.CurrencyCode == code)
                .Select(c => (int?)c.DecimalDigits)
                .FirstOrDefaultAsync(ct) ?? (string.Equals(code, "IRR", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(code, "IRT", StringComparison.OrdinalIgnoreCase) ? 0 : 2);
            return new PriceListRateSnapshot
            {
                CurrencyCode = code,
                UsdToCurrency = 1m,
                DecimalDigits = EffectiveDigits(code, digits),
                HasRate = false,
            };
        }

        var rateDigits = rate.CurrencyCodeNavigation?.DecimalDigits ?? 2;
        return new PriceListRateSnapshot
        {
            CurrencyRateID = rate.CurrencyRateID,
            CurrencyCode = rate.CurrencyCode,
            UsdToCurrency = rate.USDToCurrency,
            LastUpdate = rate.LastUpdate,
            DecimalDigits = EffectiveDigits(rate.CurrencyCode, rateDigits),
            HasRate = true,
        };
    }

    private static int EffectiveDigits(string currencyCode, int decimalDigits) =>
        string.Equals(currencyCode, "IRR", StringComparison.OrdinalIgnoreCase)
        || string.Equals(currencyCode, "IRT", StringComparison.OrdinalIgnoreCase)
            ? 0
            : Math.Clamp(decimalDigits, 0, 6);

    private static void NormalizeList(PriceList list)
    {
        list.Title = (list.Title ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(list.Title))
            throw new ArgumentException("Title is required.");

        list.Slug = string.IsNullOrWhiteSpace(list.Slug) ? Slugify(list.Title) : Slugify(list.Slug);
        list.Description = TrimOrNull(list.Description, 2000);
        list.Notes = TrimOrNull(list.Notes, 2000);

        if (list.Source != (byte)PriceListSource.CatalogProducts)
        {
            list.Source = (byte)PriceListSource.Custom;
            list.ProductCategoryID = null;
            list.BrandID = null;
        }
        else
        {
            list.Pricing = (byte)PriceListPricing.SiteCurrency;
        }

        if (list.Pricing != (byte)PriceListPricing.LinkedToUsd)
            list.Pricing = (byte)PriceListPricing.SiteCurrency;
    }

    private static void NormalizeItem(PriceListItem item)
    {
        item.Title = (item.Title ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(item.Title))
            throw new ArgumentException("Title is required.");

        item.GroupName = TrimOrNull(item.GroupName, 150);
        item.Specification = TrimOrNull(item.Specification, 300);
        item.Unit = TrimOrNull(item.Unit, 50);
        item.Sku = TrimOrNull(item.Sku, 100);
        item.Notes = TrimOrNull(item.Notes, 1000);
        item.BasePriceUsd = decimal.Round(Math.Max(0, item.BasePriceUsd), 4, MidpointRounding.AwayFromZero);
        if (item.FixedPrice is decimal fixedPrice)
            item.FixedPrice = decimal.Round(Math.Max(0, fixedPrice), 4, MidpointRounding.AwayFromZero);
        if (!item.LinkToUsd && item.FixedPrice is null)
            item.FixedPrice = 0m;
    }

    private static async Task EnsureUniqueSlugAsync(AppDbContext _context, int websiteId, string slug, int? excludeId, CancellationToken ct)
    {
        var clash = await _context.PriceLists.AnyAsync(l =>
            l.WebsiteID == websiteId
            && l.Slug == slug
            && (excludeId == null || l.PriceListID != excludeId), ct);
        if (clash)
            throw new InvalidOperationException("A price list with this slug already exists on the website.");
    }

    private static string Slugify(string value)
    {
        var s = value.Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"\s+", "-");
        s = Regex.Replace(s, @"[^\p{L}\p{N}-]+", "");
        s = Regex.Replace(s, "-{2,}", "-").Trim('-');
        if (string.IsNullOrEmpty(s)) s = "list";
        return s.Length <= 200 ? s : s[..200].Trim('-');
    }

    private static string? TrimOrNull(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Trim();
        return value.Length <= max ? value : value[..max];
    }
}
