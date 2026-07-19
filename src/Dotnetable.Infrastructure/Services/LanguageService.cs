using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

/// <inheritdoc cref="ILanguageService"/>
public class LanguageService : ILanguageService
{
    // Seed data for a fresh install: admin catalog only (WebsiteID null).
    private static readonly (string Code, string Iso, string Name, bool Rtl)[] DefaultLanguages =
    [
        ("en", "en-US", "English",  false),
        ("de", "de-DE", "Deutsch",  false),
        ("fr", "fr-FR", "Français", false),
        ("ru", "ru-RU", "Русский",  false),
        ("zh", "zh-CN", "中文",      false),
        ("fa", "fa-IR", "فارسی",    true),
        ("ar", "ar-SA", "العربية",  true),
    ];

    private readonly AppDbContext _context;
    private readonly LanguageCatalogCache _cache;

    public LanguageService(AppDbContext context, LanguageCatalogCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // Cached behind a singleton lock: components across the same Blazor circuit share one scoped
    // AppDbContext — without serializing the initial fetch, concurrent loads race and throw.
    public Task<List<Language>> GetCatalogAsync(CancellationToken ct = default) =>
        _cache.GetOrLoadCatalogAsync(() => LoadCatalogAsync(ct));

    public async Task<PagedResult<Language>> GetCatalogPagedAsync(GridQuery query, CancellationToken ct = default)
    {
        var q = (await GetCatalogAsync(ct)).AsQueryable();

        if (query.GetSearch(nameof(Language.LanguageCode)) is string code)
            q = q.Where(l => l.LanguageCode.Contains(code, StringComparison.OrdinalIgnoreCase));
        if (query.GetSearch(nameof(Language.LanguageCodeISO)) is string iso)
            q = q.Where(l => l.LanguageCodeISO.Contains(iso, StringComparison.OrdinalIgnoreCase));
        if (query.GetSearch(nameof(Language.Name)) is string name)
            q = q.Where(l => l.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

        var total = q.Count();
        var items = q.ApplyOrderBy(query.OrderBy, nameof(Language.Priority)).Skip(query.Skip).Take(query.Take).ToList();

        return new PagedResult<Language> { Items = items, TotalCount = total };
    }

    private async Task<List<Language>> LoadCatalogAsync(CancellationToken ct)
    {
        var catalog = await _context.Languages
            .AsNoTracking()
            .Where(l => l.WebsiteID == null)
            .OrderBy(l => l.Priority)
            .ToListAsync(ct);

        if (catalog.Count > 0) return catalog;

        return await SeedAdminCatalogAsync(ct);
    }

    public async Task<List<Language>> GetActiveCatalogAsync(CancellationToken ct = default) =>
        (await GetCatalogAsync(ct)).Where(l => l.Active).ToList();

    public async Task<List<Language>> GetOtherActiveCatalogAsync(CancellationToken ct = default) =>
        (await GetActiveCatalogAsync(ct)).Where(l => !l.IsDefault).ToList();

    public async Task<Language> CreateAsync(Language language, CancellationToken ct = default)
    {
        language.WebsiteID = null;
        language.LanguageCode = language.LanguageCode.Trim().ToLowerInvariant();

        if (language.IsDefault)
            await ClearExistingDefaultAsync(null, ct);

        _context.Languages.Add(language);
        await _context.SaveChangesAsync(ct);
        _cache.InvalidateAll();
        return language;
    }

    public async Task<bool> UpdateAsync(Language language, CancellationToken ct = default)
    {
        var existing = await _context.Languages.FirstOrDefaultAsync(
            l => l.LanguageID == language.LanguageID && l.WebsiteID == null, ct);
        if (existing is null) return false;

        if (language.IsDefault && !existing.IsDefault)
            await ClearExistingDefaultAsync(null, ct);

        if (existing.IsDefault || language.IsDefault)
            language.Active = true;

        existing.Name = language.Name;
        existing.LanguageCodeISO = language.LanguageCodeISO;
        existing.Priority = language.Priority;
        existing.IsDefault = language.IsDefault || existing.IsDefault;
        existing.Active = language.Active;
        existing.RTLDesign = language.RTLDesign;

        await _context.SaveChangesAsync(ct);
        _cache.InvalidateAll();
        return true;
    }

    public async Task<bool> SetActiveAsync(int languageId, bool active, CancellationToken ct = default)
    {
        var existing = await _context.Languages.FirstOrDefaultAsync(
            l => l.LanguageID == languageId && l.WebsiteID == null, ct);
        if (existing is null) return false;

        if (!active && existing.IsDefault)
            throw new InvalidOperationException("The default language cannot be deactivated.");

        existing.Active = active;
        await _context.SaveChangesAsync(ct);
        _cache.InvalidateAll();
        return true;
    }

    // ── Per-website languages ────────────────────────────────────────────────

    public Task<List<Language>> GetForWebsiteAsync(int websiteId, CancellationToken ct = default) =>
        _cache.GetOrLoadWebsiteAsync(websiteId, () => LoadForWebsiteAsync(websiteId, ct));

    public async Task<List<Language>> GetActiveForWebsiteAsync(int websiteId, CancellationToken ct = default) =>
        (await GetForWebsiteAsync(websiteId, ct)).Where(l => l.Active).ToList();

    public async Task<List<Language>> GetOtherActiveForWebsiteAsync(int websiteId, CancellationToken ct = default)
    {
        var active = await GetActiveForWebsiteAsync(websiteId, ct);
        if (active.Count <= 1) return [];

        var defaultLang = active.FirstOrDefault(l => l.IsDefault)
            ?? active.OrderBy(l => l.Priority).ThenBy(l => l.Name).First();

        return active.Where(l => l.LanguageID != defaultLang.LanguageID).ToList();
    }

    private async Task<List<Language>> LoadForWebsiteAsync(int websiteId, CancellationToken ct)
    {
        await EnsureWebsiteDefaultLanguageAsync(websiteId, ct);

        return await _context.Languages
            .AsNoTracking()
            .Where(l => l.WebsiteID == websiteId)
            .OrderBy(l => l.Priority)
            .ThenBy(l => l.Name)
            .ToListAsync(ct);
    }

    public async Task<Language> AddWebsiteLanguageAsync(int websiteId, Language language, CancellationToken ct = default)
    {
        language.WebsiteID = websiteId;
        language.LanguageCode = language.LanguageCode.Trim().ToLowerInvariant();
        language.LanguageCodeISO = string.IsNullOrWhiteSpace(language.LanguageCodeISO)
            ? language.LanguageCode
            : language.LanguageCodeISO.Trim();
        language.Active = true;

        var exists = await _context.Languages.AnyAsync(
            l => l.WebsiteID == websiteId && l.LanguageCode == language.LanguageCode, ct);
        if (exists)
            throw new InvalidOperationException($"'{language.LanguageCode}' has already been added to this website.");

        var hasDefault = await _context.Languages.AnyAsync(l => l.WebsiteID == websiteId && l.IsDefault, ct);
        if (!hasDefault || language.IsDefault)
        {
            if (language.IsDefault || !hasDefault)
            {
                await ClearExistingDefaultAsync(websiteId, ct);
                language.IsDefault = true;
            }
        }

        if (language.Priority == 0)
        {
            var maxPriority = await _context.Languages
                .Where(l => l.WebsiteID == websiteId)
                .Select(l => (int?)l.Priority)
                .MaxAsync(ct) ?? -1;
            language.Priority = maxPriority + 1;
        }

        _context.Languages.Add(language);
        await _context.SaveChangesAsync(ct);
        _cache.InvalidateAll();
        return language;
    }

    public async Task<bool> UpdateWebsiteLanguageAsync(int websiteId, Language language, CancellationToken ct = default)
    {
        var existing = await _context.Languages.FirstOrDefaultAsync(
            l => l.LanguageID == language.LanguageID && l.WebsiteID == websiteId, ct);
        if (existing is null) return false;

        if (language.IsDefault && !existing.IsDefault)
            await ClearExistingDefaultAsync(websiteId, ct);

        if (existing.IsDefault || language.IsDefault)
            language.Active = true;

        existing.Name = language.Name;
        existing.LanguageCodeISO = language.LanguageCodeISO.Trim();
        existing.Priority = language.Priority;
        existing.IsDefault = language.IsDefault || existing.IsDefault;
        existing.Active = language.Active;
        existing.RTLDesign = language.RTLDesign;

        await _context.SaveChangesAsync(ct);
        _cache.InvalidateAll();
        return true;
    }

    public async Task<bool> SetWebsiteLanguageActiveAsync(int websiteId, int languageId, bool active, CancellationToken ct = default)
    {
        var existing = await _context.Languages.FirstOrDefaultAsync(
            l => l.LanguageID == languageId && l.WebsiteID == websiteId, ct);
        if (existing is null) return false;

        if (!active && existing.IsDefault)
            throw new InvalidOperationException("The default language cannot be deactivated.");

        existing.Active = active;
        await _context.SaveChangesAsync(ct);
        _cache.InvalidateAll();
        return true;
    }

    public async Task<bool> SetWebsiteDefaultLanguageAsync(int websiteId, int languageId, CancellationToken ct = default)
    {
        var existing = await _context.Languages.FirstOrDefaultAsync(
            l => l.LanguageID == languageId && l.WebsiteID == websiteId, ct);
        if (existing is null) return false;

        await ClearExistingDefaultAsync(websiteId, ct);
        existing.IsDefault = true;
        existing.Active = true;

        var website = await _context.Websites.FirstOrDefaultAsync(w => w.WebsiteID == websiteId, ct);
        if (website is not null)
            website.DefaultLanguageCode = existing.LanguageCode;

        await _context.SaveChangesAsync(ct);
        _cache.InvalidateAll();
        return true;
    }

    public async Task<bool> RemoveWebsiteLanguageAsync(int websiteId, string languageCode, CancellationToken ct = default)
    {
        var code = languageCode.Trim().ToLowerInvariant();
        var existing = await _context.Languages.FirstOrDefaultAsync(
            l => l.WebsiteID == websiteId && l.LanguageCode == code, ct);
        if (existing is null) return false;

        if (existing.IsDefault)
            throw new InvalidOperationException("The default language cannot be removed. Set another default first.");

        var count = await _context.Languages.CountAsync(l => l.WebsiteID == websiteId, ct);
        if (count <= 1)
            throw new InvalidOperationException("A website must keep at least one language.");

        _context.Languages.Remove(existing);
        await _context.SaveChangesAsync(ct);
        _cache.InvalidateAll();
        return true;
    }

    /// <summary>
    /// When a website has no language rows yet, create a single active default from
    /// <see cref="Website.DefaultLanguageCode"/> (or "en"). Never copies the admin catalog.
    /// </summary>
    private async Task EnsureWebsiteDefaultLanguageAsync(int websiteId, CancellationToken ct)
    {
        var any = await _context.Languages.AnyAsync(l => l.WebsiteID == websiteId, ct);
        if (any) return;

        var website = await _context.Websites.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebsiteID == websiteId, ct);
        var code = string.IsNullOrWhiteSpace(website?.DefaultLanguageCode)
            ? "en"
            : website!.DefaultLanguageCode.Trim().ToLowerInvariant();

        var (iso, name, rtl) = ResolveLanguageMeta(code);

        _context.Languages.Add(new Language
        {
            WebsiteID = websiteId,
            LanguageCode = code,
            LanguageCodeISO = iso,
            Name = name,
            Priority = 0,
            Active = true,
            IsDefault = true,
            RTLDesign = rtl,
        });
        await _context.SaveChangesAsync(ct);
        _cache.InvalidateAll();
    }

    private static (string Iso, string Name, bool Rtl) ResolveLanguageMeta(string code)
    {
        var known = DefaultLanguages.FirstOrDefault(l =>
            string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));
        if (known.Code is not null)
            return (known.Iso, known.Name, known.Rtl);

        return (code, code.ToUpperInvariant(), false);
    }

    /// <param name="websiteId">Null clears default on the admin catalog; non-null on that website only.</param>
    private async Task ClearExistingDefaultAsync(int? websiteId, CancellationToken ct)
    {
        var current = await _context.Languages
            .Where(l => l.WebsiteID == websiteId && l.IsDefault)
            .ToListAsync(ct);
        foreach (var l in current) l.IsDefault = false;
    }

    private async Task<List<Language>> SeedAdminCatalogAsync(CancellationToken ct)
    {
        var rows = DefaultLanguages.Select((l, i) => new Language
        {
            WebsiteID = null,
            LanguageCode = l.Code,
            LanguageCodeISO = l.Iso,
            Name = l.Name,
            Priority = i,
            Active = true,
            IsDefault = l.Code == "en",
            RTLDesign = l.Rtl,
        }).ToList();

        _context.Languages.AddRange(rows);
        await _context.SaveChangesAsync(ct);
        return rows;
    }
}
