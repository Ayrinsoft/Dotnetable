using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Text;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class PageService : IPageService
{
    private const int SlugMaxLength = 300;

    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public PageService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    /// <summary>All slugs in use by this website's pages (main + translation rows), excluding
    /// <paramref name="excludePageId"/> — the lookup route matches either, so uniqueness must span
    /// both.</summary>
    private static async Task<HashSet<string>> GetUsedSlugsAsync(
        AppDbContext context, int websiteId, int excludePageId, CancellationToken ct)
    {
        var main = await context.Pages.AsNoTracking()
            .Where(p => p.WebsiteID == websiteId && p.PageID != excludePageId)
            .Select(p => p.Slug).ToListAsync(ct);
        var translated = await context.PageTranslations.AsNoTracking()
            .Where(t => t.Page.WebsiteID == websiteId && t.PageID != excludePageId)
            .Select(t => t.Slug).ToListAsync(ct);
        return new HashSet<string>(main.Concat(translated), StringComparer.OrdinalIgnoreCase);
    }

    // ── Admin management ────────────────────────────────────────────

    public async Task<PagedResult<Page>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

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
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.Pages.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(p => p.WebsiteID == wid);
        return await q.OrderBy(p => p.SortOrder).ThenBy(p => p.Title).ToListAsync(ct);
    }

    public async Task<Page?> GetByIdAsync(int pageId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.Pages.FindAsync([pageId], ct);
    }

    public async Task<Page> CreateAsync(Page page, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        page.CreatedAt = page.UpdatedAt = DateTime.UtcNow;
        if (page.IsHomepage)
            await ClearHomepageAsync(_context, page.WebsiteID, ct);

        var used = await GetUsedSlugsAsync(_context, page.WebsiteID, excludePageId: 0, ct);
        page.Slug = SlugGenerator.MakeUnique(
            SlugGenerator.Normalize(string.IsNullOrWhiteSpace(page.Slug) ? page.Title : page.Slug, SlugMaxLength),
            used);

        _context.Pages.Add(page);
        await _context.SaveChangesAsync(ct);
        return page;
    }

    public async Task UpdateAsync(Page page, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        page.UpdatedAt = DateTime.UtcNow;
        if (page.IsHomepage)
            await ClearHomepageAsync(_context, page.WebsiteID, ct, exceptPageId: page.PageID);

        var used = await GetUsedSlugsAsync(_context, page.WebsiteID, page.PageID, ct);
        page.Slug = SlugGenerator.MakeUnique(
            SlugGenerator.Normalize(string.IsNullOrWhiteSpace(page.Slug) ? page.Title : page.Slug, SlugMaxLength),
            used);

        _context.Pages.Update(page);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int pageId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

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

    public async Task SetActiveAsync(int pageId, bool active, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        await _context.Pages.Where(p => p.PageID == pageId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, active), ct);
    }

    private async Task ClearHomepageAsync(AppDbContext _context, int websiteId, CancellationToken ct, int? exceptPageId = null)
    {
        await _context.Pages
            .Where(p => p.WebsiteID == websiteId && p.IsHomepage && (exceptPageId == null || p.PageID != exceptPageId))
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsHomepage, false), ct);
    }

    // ── Translations ────────────────────────────────────────────────

    public async Task<List<PageTranslation>> GetTranslationsAsync(int pageId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.PageTranslations.AsNoTracking()
            .Where(t => t.PageID == pageId)
            .ToListAsync(ct);
    }

    public async Task SetTranslationsAsync(int pageId, IReadOnlyList<PageTranslation> translations, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var page = await _context.Pages.AsNoTracking().FirstOrDefaultAsync(p => p.PageID == pageId, ct);
        if (page is null) return;

        var existing = await _context.PageTranslations.Where(t => t.PageID == pageId).ToListAsync(ct);

        // Remove languages that are no longer present or were cleared.
        var keepLanguages = translations
            .Where(t => !string.IsNullOrWhiteSpace(t.Title))
            .Select(t => t.LanguageCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _context.PageTranslations.RemoveRange(existing.Where(t => !keepLanguages.Contains(t.LanguageCode)));

        var used = await GetUsedSlugsAsync(_context, page.WebsiteID, pageId, ct);

        foreach (var t in translations)
        {
            if (string.IsNullOrWhiteSpace(t.Title)) continue;
            var current = existing.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, t.LanguageCode, StringComparison.OrdinalIgnoreCase));
            var slug = SlugGenerator.MakeUnique(
                SlugGenerator.Normalize(string.IsNullOrWhiteSpace(t.Slug) ? t.Title : t.Slug, SlugMaxLength),
                used);
            used.Add(slug);

            if (current is null)
                _context.PageTranslations.Add(new PageTranslation
                {
                    PageID = pageId,
                    LanguageCode = t.LanguageCode,
                    Title = t.Title.Trim(),
                    Slug = slug,
                    Content = t.Content,
                    MetaTitle = t.MetaTitle,
                    MetaDescription = t.MetaDescription,
                    MetaKeywords = t.MetaKeywords,
                });
            else
            {
                current.Title = t.Title.Trim();
                current.Slug = slug;
                current.Content = t.Content;
                current.MetaTitle = t.MetaTitle;
                current.MetaDescription = t.MetaDescription;
                current.MetaKeywords = t.MetaKeywords;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    // ── Public read ─────────────────────────────────────────────────

    public async Task<List<PageDto>> GetTreeAsync(int websiteId, string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var pages = await _context.Pages.AsNoTracking()
            .Where(p => p.WebsiteID == websiteId && p.IsActive && p.Status == 1)
            .Include(p => p.PageTranslations)
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Title)
            .ToListAsync(ct);

        var byParent = pages.ToLookup(p => p.ParentPageID);
        return BuildChildren(null, byParent, languageCode);
    }

    public async Task<PageDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var page = await _context.Pages.AsNoTracking()
            .Include(p => p.PageTranslations)
            .FirstOrDefaultAsync(p => p.WebsiteID == websiteId && p.IsActive && p.Status == 1 &&
                (p.Slug == slug || p.PageTranslations.Any(t => t.Slug == slug)), ct);
        return page is null ? null : Project(page, Array.Empty<PageDto>(), languageCode);
    }

    public async Task<PageDto?> GetHomepageAsync(int websiteId, string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var page = await _context.Pages.AsNoTracking()
            .Include(p => p.PageTranslations)
            .FirstOrDefaultAsync(p => p.WebsiteID == websiteId && p.IsActive && p.Status == 1 && p.IsHomepage, ct);
        return page is null ? null : Project(page, Array.Empty<PageDto>(), languageCode);
    }

    // ── Projection helpers ──────────────────────────────────────────

    private static List<PageDto> BuildChildren(
        int? parentId,
        ILookup<int?, Page> byParent,
        string? languageCode)
    {
        return byParent[parentId]
            .Select(p => Project(p, BuildChildren(p.PageID, byParent, languageCode), languageCode))
            .ToList();
    }

    private static PageDto Project(Page p, IReadOnlyList<PageDto> children, string? languageCode)
    {
        var (title, slug, content) = Localized(p, languageCode);
        var (metaTitle, metaDescription, metaKeywords) = LocalizedMeta(p, languageCode);
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
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = metaKeywords,
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

    private static (string? MetaTitle, string? MetaDescription, string? MetaKeywords) LocalizedMeta(Page p, string? languageCode)
    {
        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            var t = p.PageTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));
            if (t is not null)
                return (
                    string.IsNullOrWhiteSpace(t.MetaTitle) ? p.MetaTitle : t.MetaTitle,
                    string.IsNullOrWhiteSpace(t.MetaDescription) ? p.MetaDescription : t.MetaDescription,
                    string.IsNullOrWhiteSpace(t.MetaKeywords) ? p.MetaKeywords : t.MetaKeywords);
        }
        return (p.MetaTitle, p.MetaDescription, p.MetaKeywords);
    }
}
