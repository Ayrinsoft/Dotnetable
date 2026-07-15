using Dotnetable.Application;
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
    // Seed data for a fresh install / an existing database that predates this feature (the
    // Languages table exists in every migration but was never populated until now).
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

    // Cached behind a singleton lock: components across the same Blazor circuit (MainLayout,
    // NavMenu, PageLocalizer, the page itself) all read this on first render and share one scoped
    // AppDbContext — without serializing the initial fetch, two of them racing to query it at once
    // throws "A second operation was started on this context instance before a previous operation
    // completed."
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
            .Where(l => l.WebsiteID == AppConstants.MasterWebsiteId)
            .OrderBy(l => l.Priority)
            .ToListAsync(ct);

        if (catalog.Count > 0) return catalog;

        return await SeedDefaultsAsync(ct);
    }

    public async Task<List<Language>> GetActiveCatalogAsync(CancellationToken ct = default) =>
        (await GetCatalogAsync(ct)).Where(l => l.Active).ToList();

    public async Task<Language> CreateAsync(Language language, CancellationToken ct = default)
    {
        language.WebsiteID = AppConstants.MasterWebsiteId;
        language.LanguageCode = language.LanguageCode.Trim().ToLowerInvariant();

        if (language.IsDefault)
            await ClearExistingDefaultAsync(ct);

        _context.Languages.Add(language);
        await _context.SaveChangesAsync(ct);
        _cache.InvalidateAll();
        return language;
    }

    public async Task<bool> UpdateAsync(Language language, CancellationToken ct = default)
    {
        var existing = await _context.Languages.FirstOrDefaultAsync(
            l => l.LanguageID == language.LanguageID && l.WebsiteID == AppConstants.MasterWebsiteId, ct);
        if (existing is null) return false;

        if (language.IsDefault && !existing.IsDefault)
            await ClearExistingDefaultAsync(ct);

        existing.Name = language.Name;
        existing.LanguageCodeISO = language.LanguageCodeISO;
        existing.Priority = language.Priority;
        existing.IsDefault = language.IsDefault;
        existing.Active = language.Active;
        existing.RTLDesign = language.RTLDesign;

        await _context.SaveChangesAsync(ct);
        _cache.InvalidateAll();
        return true;
    }

    public async Task<bool> SetActiveAsync(int languageId, bool active, CancellationToken ct = default)
    {
        var existing = await _context.Languages.FirstOrDefaultAsync(
            l => l.LanguageID == languageId && l.WebsiteID == AppConstants.MasterWebsiteId, ct);
        if (existing is null) return false;

        existing.Active = active;
        await _context.SaveChangesAsync(ct);
        _cache.InvalidateAll();
        return true;
    }

    public Task<List<Language>> GetActiveForWebsiteAsync(int websiteId, CancellationToken ct = default) =>
        _cache.GetOrLoadWebsiteAsync(websiteId, () => LoadActiveForWebsiteAsync(websiteId, ct));

    private async Task<List<Language>> LoadActiveForWebsiteAsync(int websiteId, CancellationToken ct)
    {
        if (websiteId == AppConstants.MasterWebsiteId)
            return await GetActiveCatalogAsync(ct);

        var own = await _context.Languages
            .AsNoTracking()
            .Where(l => l.WebsiteID == websiteId && l.Active)
            .OrderBy(l => l.Priority)
            .ToListAsync(ct);
        if (own.Count > 0) return own;

        var website = await _context.Websites.AsNoTracking().FirstOrDefaultAsync(w => w.WebsiteID == websiteId, ct);
        var fallbackCode = string.IsNullOrWhiteSpace(website?.DefaultLanguageCode) ? "en" : website!.DefaultLanguageCode;
        var fallback = (await GetCatalogAsync(ct)).FirstOrDefault(l => l.LanguageCode == fallbackCode)
            ?? new Language { LanguageCode = fallbackCode, LanguageCodeISO = fallbackCode, Name = fallbackCode, Active = true, IsDefault = true };
        return [fallback];
    }

    public async Task SetWebsiteLanguagesAsync(int websiteId, IEnumerable<string> codes, CancellationToken ct = default)
    {
        if (websiteId == AppConstants.MasterWebsiteId) return;

        var activeCatalog = await GetActiveCatalogAsync(ct);
        var selected = codes.Select(c => c.Trim().ToLowerInvariant()).ToHashSet();
        var validCodes = activeCatalog.Select(l => l.LanguageCode).ToHashSet();
        selected.IntersectWith(validCodes);

        var existing = await _context.Languages.Where(l => l.WebsiteID == websiteId).ToListAsync(ct);

        foreach (var row in existing)
            row.Active = selected.Contains(row.LanguageCode);

        var toAdd = selected.Except(existing.Select(l => l.LanguageCode));
        foreach (var code in toAdd)
        {
            var catalogEntry = activeCatalog.First(l => l.LanguageCode == code);
            _context.Languages.Add(new Language
            {
                WebsiteID = websiteId,
                LanguageCode = catalogEntry.LanguageCode,
                LanguageCodeISO = catalogEntry.LanguageCodeISO,
                Name = catalogEntry.Name,
                Priority = catalogEntry.Priority,
                RTLDesign = catalogEntry.RTLDesign,
                Active = true,
                IsDefault = false,
            });
        }

        await _context.SaveChangesAsync(ct);
        _cache.InvalidateAll();
    }

    private async Task ClearExistingDefaultAsync(CancellationToken ct)
    {
        var current = await _context.Languages
            .Where(l => l.WebsiteID == AppConstants.MasterWebsiteId && l.IsDefault)
            .ToListAsync(ct);
        foreach (var l in current) l.IsDefault = false;
    }

    private async Task<List<Language>> SeedDefaultsAsync(CancellationToken ct)
    {
        var rows = DefaultLanguages.Select((l, i) => new Language
        {
            WebsiteID = AppConstants.MasterWebsiteId,
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
