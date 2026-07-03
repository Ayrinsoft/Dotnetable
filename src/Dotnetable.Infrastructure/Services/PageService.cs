using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class PageService : IPageService
{
    private readonly AppDbContext _context;

    public PageService(AppDbContext context) => _context = context;

    // ── Admin management ────────────────────────────────────────────

    public async Task<PagedResult<Page>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Pages.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(p => p.WebsiteID == wid);

        if (query.GetSearch(nameof(Page.Title)) is string title)
            q = q.Where(p => p.Title.Contains(title));
        if (query.GetSearch(nameof(Page.Slug)) is string slug)
            q = q.Where(p => p.Slug.Contains(slug));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Page.SortOrder))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Page> { Items = items, TotalCount = total };
    }

    public async Task<List<Page>> GetAllAsync(int? websiteId, CancellationToken ct = default)
    {
        var q = _context.Pages.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(p => p.WebsiteID == wid);
        return await q.OrderBy(p => p.SortOrder).ThenBy(p => p.Title).ToListAsync(ct);
    }

    public async Task<Page?> GetByIdAsync(int pageId, CancellationToken ct = default) =>
        await _context.Pages.FindAsync([pageId], ct);

    public async Task<Page> CreateAsync(Page page, CancellationToken ct = default)
    {
        page.CreatedAt = page.UpdatedAt = DateTime.UtcNow;
        if (page.IsHomepage)
            await ClearHomepageAsync(page.WebsiteID, ct);
        _context.Pages.Add(page);
        await _context.SaveChangesAsync(ct);
        return page;
    }

    public async Task UpdateAsync(Page page, CancellationToken ct = default)
    {
        page.UpdatedAt = DateTime.UtcNow;
        if (page.IsHomepage)
            await ClearHomepageAsync(page.WebsiteID, ct, exceptPageId: page.PageID);
        _context.Pages.Update(page);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int pageId, CancellationToken ct = default)
    {
        var page = await _context.Pages
            .Include(p => p.PageTranslations)
            .Include(p => p.InverseParentPage)
            .Include(p => p.MenuItems)
            .FirstOrDefaultAsync(p => p.PageID == pageId, ct);
        if (page is null) return;

        foreach (var child in page.InverseParentPage)
            child.ParentPageID = page.ParentPageID;
        foreach (var mi in page.MenuItems)
            mi.PageID = null;

        _context.PageTranslations.RemoveRange(page.PageTranslations);
        _context.Pages.Remove(page);
        await _context.SaveChangesAsync(ct);
    }

    public async Task SetActiveAsync(int pageId, bool active, CancellationToken ct = default) =>
        await _context.Pages.Where(p => p.PageID == pageId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, active), ct);

    private async Task ClearHomepageAsync(int websiteId, CancellationToken ct, int? exceptPageId = null) =>
        await _context.Pages
            .Where(p => p.WebsiteID == websiteId && p.IsHomepage && (exceptPageId == null || p.PageID != exceptPageId))
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsHomepage, false), ct);

    // ── Translations ────────────────────────────────────────────────

    public async Task<List<PageTranslation>> GetTranslationsAsync(int pageId, CancellationToken ct = default) =>
        await _context.PageTranslations.AsNoTracking()
            .Where(t => t.PageID == pageId)
            .ToListAsync(ct);

    public async Task SetTranslationsAsync(int pageId, IReadOnlyList<PageTranslation> translations, CancellationToken ct = default)
    {
        var existing = await _context.PageTranslations.Where(t => t.PageID == pageId).ToListAsync(ct);

        // Remove languages that are no longer present or were cleared.
        var keepLanguages = translations
            .Where(t => !string.IsNullOrWhiteSpace(t.Title))
            .Select(t => t.LanguageCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _context.PageTranslations.RemoveRange(existing.Where(t => !keepLanguages.Contains(t.LanguageCode)));

        foreach (var t in translations)
        {
            if (string.IsNullOrWhiteSpace(t.Title)) continue;
            var current = existing.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, t.LanguageCode, StringComparison.OrdinalIgnoreCase));
            var slug = string.IsNullOrWhiteSpace(t.Slug) ? t.Title.Trim() : t.Slug.Trim();

            if (current is null)
                _context.PageTranslations.Add(new PageTranslation
                {
                    PageID = pageId,
                    LanguageCode = t.LanguageCode,
                    Title = t.Title.Trim(),
                    Slug = slug,
                    Content = t.Content,
                });
            else
            {
                current.Title = t.Title.Trim();
                current.Slug = slug;
                current.Content = t.Content;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    // ── Public read ─────────────────────────────────────────────────

    public async Task<List<PageDto>> GetTreeAsync(int websiteId, string? languageCode = null, CancellationToken ct = default)
    {
        var pages = await _context.Pages.AsNoTracking()
            .Where(p => p.WebsiteID == websiteId && p.IsActive && p.Status == 1)
            .Include(p => p.PageTranslations)
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Title)
            .ToListAsync(ct);

        var byParent = pages.GroupBy(p => p.ParentPageID).ToDictionary(g => g.Key, g => g.ToList());
        return BuildChildren(null, byParent, languageCode);
    }

    public async Task<PageDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default)
    {
        var page = await _context.Pages.AsNoTracking()
            .Include(p => p.PageTranslations)
            .FirstOrDefaultAsync(p => p.WebsiteID == websiteId && p.IsActive && p.Status == 1 &&
                (p.Slug == slug || p.PageTranslations.Any(t => t.Slug == slug)), ct);
        return page is null ? null : Project(page, Array.Empty<PageDto>(), languageCode);
    }

    public async Task<PageDto?> GetHomepageAsync(int websiteId, string? languageCode = null, CancellationToken ct = default)
    {
        var page = await _context.Pages.AsNoTracking()
            .Include(p => p.PageTranslations)
            .FirstOrDefaultAsync(p => p.WebsiteID == websiteId && p.IsActive && p.Status == 1 && p.IsHomepage, ct);
        return page is null ? null : Project(page, Array.Empty<PageDto>(), languageCode);
    }

    // ── Projection helpers ──────────────────────────────────────────

    private static List<PageDto> BuildChildren(
        int? parentId,
        IReadOnlyDictionary<int?, List<Page>> byParent,
        string? languageCode)
    {
        if (!byParent.TryGetValue(parentId, out var children))
            return new List<PageDto>();

        return children
            .Select(p => Project(p, BuildChildren(p.PageID, byParent, languageCode), languageCode))
            .ToList();
    }

    private static PageDto Project(Page p, IReadOnlyList<PageDto> children, string? languageCode)
    {
        var (title, slug, content) = Localized(p, languageCode);
        return new PageDto
        {
            PageID = p.PageID,
            ParentPageID = p.ParentPageID,
            Title = title,
            Slug = slug,
            Content = content,
            Template = p.Template,
            IsHomepage = p.IsHomepage,
            SortOrder = p.SortOrder,
            Children = children,
        };
    }

    private static (string Title, string Slug, string? Content) Localized(Page p, string? languageCode)
    {
        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            var t = p.PageTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.Title))
                return (t.Title, string.IsNullOrWhiteSpace(t.Slug) ? p.Slug : t.Slug,
                    string.IsNullOrWhiteSpace(t.Content) ? p.Content : t.Content);
        }
        return (p.Title, p.Slug, p.Content);
    }
}
