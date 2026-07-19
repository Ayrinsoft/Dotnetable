using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ProductCategoryService : IProductCategoryService
{
    private readonly AppDbContext _context;

    public ProductCategoryService(AppDbContext context) => _context = context;

    // ── Admin management ────────────────────────────────────────────

    public async Task<List<ProductCategory>> GetAllAsync(int? websiteId, CancellationToken ct = default)
    {
        var q = _context.ProductCategories.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(c => c.WebsiteID == wid);
        return await q.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync(ct);
    }

    public async Task<PagedResult<ProductCategory>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.ProductCategories.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(c => c.WebsiteID == wid);

        if (query.GetSearch(nameof(ProductCategory.Name)) is string name)
            q = q.Where(c => c.Name.Contains(name));
        if (query.GetSearch(nameof(ProductCategory.Slug)) is string slug)
            q = q.Where(c => c.Slug.Contains(slug));
        if (query.GetSearch(nameof(ProductCategory.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(c => c.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(ProductCategory.SortOrder))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<ProductCategory> { Items = items, TotalCount = total };
    }

    public async Task<ProductCategory?> GetByIdAsync(int productCategoryId, CancellationToken ct = default) =>
        await _context.ProductCategories.FindAsync([productCategoryId], ct);

    public async Task<ProductCategory> CreateAsync(ProductCategory category, CancellationToken ct = default)
    {
        NormalizeOptionalFks(category);
        DetachTracked(_context.ProductCategories, category.ProductCategoryID, c => c.ProductCategoryID);
        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync(ct);
        // Blazor Server keeps AppDbContext for the whole circuit; detach so a later
        // edit of the same key with a fresh copy does not hit an identity conflict.
        _context.Entry(category).State = EntityState.Detached;
        return category;
    }

    public async Task UpdateAsync(ProductCategory category, CancellationToken ct = default)
    {
        NormalizeOptionalFks(category);
        DetachTracked(_context.ProductCategories, category.ProductCategoryID, c => c.ProductCategoryID);
        _context.ProductCategories.Update(category);
        await _context.SaveChangesAsync(ct);
        _context.Entry(category).State = EntityState.Detached;
    }

    public async Task DeleteAsync(int productCategoryId, CancellationToken ct = default)
    {
        DetachTracked(_context.ProductCategories, productCategoryId, c => c.ProductCategoryID);
        var category = await _context.ProductCategories
            .Include(c => c.ProductCategoryTranslations)
            .Include(c => c.ProductCategoryMaps)
            .Include(c => c.InverseParentCategory)
            .FirstOrDefaultAsync(c => c.ProductCategoryID == productCategoryId, ct);
        if (category is null) return;

        // Re-parent children so the self FK never dangles, drop product links and translations.
        foreach (var child in category.InverseParentCategory)
            child.ParentCategoryID = category.ParentCategoryID;

        _context.ProductCategoryMaps.RemoveRange(category.ProductCategoryMaps);
        _context.ProductCategoryTranslations.RemoveRange(category.ProductCategoryTranslations);
        _context.ProductCategories.Remove(category);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>MudSelect / clearable binds sometimes emit 0 instead of null for optional int FKs.</summary>
    private static void NormalizeOptionalFks(ProductCategory category)
    {
        if (category.ParentCategoryID is 0) category.ParentCategoryID = null;
    }

    /// <summary>Detaches any stale tracked instance with the same key before an Add/Update. AppDbContext is
    /// scoped per Blazor Server circuit (not per request), so an entity saved earlier in the same session
    /// stays tracked and would otherwise collide with a fresh detached copy carrying the same primary key.</summary>
    private void DetachTracked<TEntity>(DbSet<TEntity> set, int key, Func<TEntity, int> keySelector) where TEntity : class
    {
        if (key == 0) return;
        var local = set.Local.FirstOrDefault(e => keySelector(e) == key);
        if (local is not null)
            _context.Entry(local).State = EntityState.Detached;
    }

    // ── Translations ────────────────────────────────────────────────

    public async Task<List<ProductCategoryTranslation>> GetTranslationsAsync(int productCategoryId, CancellationToken ct = default) =>
        await _context.ProductCategoryTranslations.AsNoTracking()
            .Where(t => t.ProductCategoryID == productCategoryId)
            .ToListAsync(ct);

    public async Task SetTranslationsAsync(int productCategoryId, IReadOnlyDictionary<string, (string Name, string Slug)> byLanguage, CancellationToken ct = default)
    {
        var existing = await _context.ProductCategoryTranslations
            .Where(t => t.ProductCategoryID == productCategoryId)
            .ToListAsync(ct);

        foreach (var (languageCode, value) in byLanguage)
        {
            var current = existing.FirstOrDefault(t =>
                string.Equals(t.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(value.Name))
            {
                if (current is not null) _context.ProductCategoryTranslations.Remove(current);
                continue;
            }

            var slug = string.IsNullOrWhiteSpace(value.Slug) ? value.Name.Trim() : value.Slug.Trim();
            if (current is null)
                _context.ProductCategoryTranslations.Add(new ProductCategoryTranslation
                {
                    ProductCategoryID = productCategoryId,
                    LanguageCode = languageCode,
                    Name = value.Name.Trim(),
                    Slug = slug,
                });
            else
            {
                current.Name = value.Name.Trim();
                current.Slug = slug;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    // ── Public read ─────────────────────────────────────────────────

    public async Task<List<ProductCategoryDto>> GetTreeAsync(int websiteId, string? languageCode = null, CancellationToken ct = default)
    {
        var categories = await _context.ProductCategories.AsNoTracking()
            .Where(c => c.WebsiteID == websiteId && c.IsActive)
            .Include(c => c.ProductCategoryTranslations)
            .Include(c => c.ImageFile)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .ToListAsync(ct);

        var byParent = categories.ToLookup(c => c.ParentCategoryID);

        return BuildChildren(null, byParent, languageCode);
    }

    public async Task<ProductCategoryDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default)
    {
        var category = await _context.ProductCategories.AsNoTracking()
            .Include(c => c.ProductCategoryTranslations)
            .Include(c => c.ImageFile)
            .FirstOrDefaultAsync(c => c.WebsiteID == websiteId && c.IsActive &&
                (c.Slug == slug || c.ProductCategoryTranslations.Any(t => t.Slug == slug)), ct);
        return category is null ? null : Project(category, Array.Empty<ProductCategoryDto>(), languageCode);
    }

    // ── Projection helpers ──────────────────────────────────────────

    private static List<ProductCategoryDto> BuildChildren(
        int? parentId,
        ILookup<int?, ProductCategory> byParent,
        string? languageCode)
    {
        return byParent[parentId]
            .Select(c => Project(c, BuildChildren(c.ProductCategoryID, byParent, languageCode), languageCode))
            .ToList();
    }

    private static ProductCategoryDto Project(ProductCategory c, IReadOnlyList<ProductCategoryDto> children, string? languageCode)
    {
        var (name, slug) = Localized(c, languageCode);
        return new ProductCategoryDto
        {
            ProductCategoryID = c.ProductCategoryID,
            ParentCategoryID = c.ParentCategoryID,
            Name = name,
            Slug = slug,
            ImageUrl = c.ImageFile?.ThumbnailCDN ?? c.ImageFile?.CNDUrl,
            SortOrder = c.SortOrder,
            Children = children,
        };
    }

    private static (string Name, string Slug) Localized(ProductCategory c, string? languageCode)
    {
        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            var t = c.ProductCategoryTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.Name))
                return (t.Name, string.IsNullOrWhiteSpace(t.Slug) ? c.Slug : t.Slug);
        }
        return (c.Name, c.Slug);
    }
}
