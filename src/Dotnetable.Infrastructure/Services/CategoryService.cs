using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context) => _context = context;

    // ── Admin management ────────────────────────────────────────────

    public async Task<List<Category>> GetAllAsync(int? websiteId, CancellationToken ct = default)
    {
        var q = _context.Categories.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(c => c.WebsiteID == wid);
        return await q.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync(ct);
    }

    public async Task<Category?> GetByIdAsync(int categoryId, CancellationToken ct = default) =>
        await _context.Categories.FindAsync([categoryId], ct);

    public async Task<Category> CreateAsync(Category category, CancellationToken ct = default)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(ct);
        return category;
    }

    public async Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int categoryId, CancellationToken ct = default)
    {
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

    // ── Translations ────────────────────────────────────────────────

    public async Task<List<CategoryTranslation>> GetTranslationsAsync(int categoryId, CancellationToken ct = default) =>
        await _context.CategoryTranslations.AsNoTracking()
            .Where(t => t.CategoryID == categoryId)
            .ToListAsync(ct);

    public async Task SetTranslationsAsync(int categoryId, IReadOnlyDictionary<string, (string Name, string Slug)> byLanguage, CancellationToken ct = default)
    {
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
