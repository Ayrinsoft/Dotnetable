using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public CategoryService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    // ── Admin management ────────────────────────────────────────────

    public async Task<List<Category>> GetAllAsync(int? websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.Categories.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(c => c.WebsiteID == wid);
        return await q.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync(ct);
    }

    public async Task<PagedResult<Category>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.Categories.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(c => c.WebsiteID == wid);

        if (query.GetSearch(nameof(Category.Name)) is string name)
            q = q.Where(c => c.Name.Contains(name));
        if (query.GetSearch(nameof(Category.Slug)) is string slug)
            q = q.Where(c => c.Slug.Contains(slug));
        if (query.GetSearch(nameof(Category.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(c => c.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Category.SortOrder))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Category> { Items = items, TotalCount = total };
    }

    public async Task<Category?> GetByIdAsync(int categoryId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.Categories.FindAsync([categoryId], ct);
    }

    public async Task<Category> CreateAsync(Category category, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        NormalizeOptionalFks(category);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(ct);
        // Blazor Server keeps AppDbContext for the whole circuit; detach so a later
        // edit of the same key with a fresh copy does not hit an identity conflict.
        _context.Entry(category).State = EntityState.Detached;
        return category;
    }

    public async Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        NormalizeOptionalFks(category);
        _context.Categories.Update(category);
        await _context.SaveChangesAsync(ct);
        _context.Entry(category).State = EntityState.Detached;
    }

    public async Task DeleteAsync(int categoryId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var category = await _context.Categories
            .Include(c => c.CategoryTranslations)
            .Include(c => c.PostCategories)
            .Include(c => c.InverseParentCategory)
            .FirstOrDefaultAsync(c => c.CategoryID == categoryId, ct);
        if (category is null) return;

        // Re-parent children so the self FK never dangles, drop post links and translations.
        foreach (var child in category.InverseParentCategory)
            child.ParentCategoryID = category.ParentCategoryID;

        _context.PostCategories.RemoveRange(category.PostCategories);
        _context.CategoryTranslations.RemoveRange(category.CategoryTranslations);
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>MudSelect / clearable binds sometimes emit 0 instead of null for optional int FKs;
    /// 0 is not a real Category/PostType key and breaks SAME TABLE / PostTypes FKs.</summary>
    private static void NormalizeOptionalFks(Category category)
    {
        if (category.ParentCategoryID is 0) category.ParentCategoryID = null;
        if (category.PostTypeID is 0) category.PostTypeID = null;
    }


    // ── Translations ────────────────────────────────────────────────

    public async Task<List<CategoryTranslation>> GetTranslationsAsync(int categoryId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.CategoryTranslations.AsNoTracking()
            .Where(t => t.CategoryID == categoryId)
            .ToListAsync(ct);
    }

    public async Task SetTranslationsAsync(int categoryId, IReadOnlyDictionary<string, (string Name, string Slug)> byLanguage, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.CategoryTranslations
            .Where(t => t.CategoryID == categoryId)
            .ToListAsync(ct);

        foreach (var (languageCode, value) in byLanguage)
        {
            var current = existing.FirstOrDefault(t =>
                string.Equals(t.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(value.Name))
            {
                if (current is not null) _context.CategoryTranslations.Remove(current);
                continue;
            }

            var slug = string.IsNullOrWhiteSpace(value.Slug) ? value.Name.Trim() : value.Slug.Trim();
            if (current is null)
                _context.CategoryTranslations.Add(new CategoryTranslation
                {
                    CategoryID = categoryId,
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

    public async Task<List<CategoryDto>> GetTreeAsync(int websiteId, int? postTypeId = null, string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.Categories.AsNoTracking()
            .Where(c => c.WebsiteID == websiteId && c.IsActive)
            .Include(c => c.CategoryTranslations)
            .AsQueryable();
        if (postTypeId is int pid)
            q = q.Where(c => c.PostTypeID == pid);

        var categories = await q.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync(ct);

        var byParent = categories.ToLookup(c => c.ParentCategoryID);

        return BuildChildren(null, byParent, languageCode);
    }

    public async Task<CategoryDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var category = await _context.Categories.AsNoTracking()
            .Include(c => c.CategoryTranslations)
            .FirstOrDefaultAsync(c => c.WebsiteID == websiteId && c.IsActive &&
                (c.Slug == slug || c.CategoryTranslations.Any(t => t.Slug == slug)), ct);
        return category is null ? null : Project(category, Array.Empty<CategoryDto>(), languageCode);
    }

    // ── Projection helpers ──────────────────────────────────────────

    private static List<CategoryDto> BuildChildren(
        int? parentId,
        ILookup<int?, Category> byParent,
        string? languageCode)
    {
        return byParent[parentId]
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => Project(c, BuildChildren(c.CategoryID, byParent, languageCode), languageCode))
            .ToList();
    }

    private static CategoryDto Project(Category c, IReadOnlyList<CategoryDto> children, string? languageCode)
    {
        var (name, slug) = Localized(c, languageCode);
        return new CategoryDto
        {
            CategoryID = c.CategoryID,
            ParentCategoryID = c.ParentCategoryID,
            PostTypeID = c.PostTypeID,
            Name = name,
            Slug = slug,
            SortOrder = c.SortOrder,
            Children = children,
        };
    }

    private static (string Name, string Slug) Localized(Category c, string? languageCode)
    {
        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            var t = c.CategoryTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.Name))
                return (t.Name, string.IsNullOrWhiteSpace(t.Slug) ? c.Slug : t.Slug);
        }
        return (c.Name, c.Slug);
    }
}
