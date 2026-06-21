using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class LocalizationService : ILocalizationService
{
    private readonly AppDbContext _context;
    private readonly TranslationCache _cache;

    public LocalizationService(AppDbContext context, TranslationCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task LoadAsync(int websiteId, string languageCode, CancellationToken ct = default)
    {
        var entries = await QueryFor(websiteId, languageCode).ToListAsync(ct);
        _cache.Load(entries.Select(e => (websiteId, languageCode, e.Key, e.Value)));
    }

    private IQueryable<KeyValue> QueryFor(int websiteId, string languageCode) =>
        _context.LocalizationKeys
            .Where(k => k.WebsiteID == websiteId)
            .Select(k => new KeyValue(
                k.ItemKey,
                k.LocalizationValues
                    .Where(v => v.LanguageCode == languageCode)
                    .Select(v => v.ItemValue)
                    .FirstOrDefault() ?? k.DefaultValue));

    private sealed record KeyValue(string Key, string Value);

    public string Get(string key, string? fallback = null) =>
        _cache.TryGet(0, string.Empty, key, out var v) ? v : fallback ?? key;

    public string Get(int websiteId, string languageCode, string key, string? fallback = null) =>
        _cache.TryGet(websiteId, languageCode, key, out var v) ? v : fallback ?? key;

    public async Task<IReadOnlyDictionary<string, string>> GetAllAsync(int websiteId, string languageCode, CancellationToken ct = default)
    {
        var entries = await QueryFor(websiteId, languageCode).ToListAsync(ct);
        return entries.ToDictionary(e => e.Key, e => e.Value);
    }

    public async Task SetAsync(int websiteId, string languageCode, string key, string value, CancellationToken ct = default)
    {
        var localizationKey = await _context.LocalizationKeys
            .Include(k => k.LocalizationValues)
            .FirstOrDefaultAsync(k => k.WebsiteID == websiteId && k.ItemKey == key, ct);

        if (localizationKey is null)
        {
            localizationKey = new LocalizationKey { WebsiteID = websiteId, ItemKey = key, DefaultValue = value };
            _context.LocalizationKeys.Add(localizationKey);
        }

        var localizedValue = localizationKey.LocalizationValues
            .FirstOrDefault(v => v.LanguageCode == languageCode);

        if (localizedValue is null)
            localizationKey.LocalizationValues.Add(new LocalizationValue { LanguageCode = languageCode, ItemValue = value });
        else
            localizedValue.ItemValue = value;

        await _context.SaveChangesAsync(ct);
        _cache.Set(websiteId, languageCode, key, value);
    }
}
