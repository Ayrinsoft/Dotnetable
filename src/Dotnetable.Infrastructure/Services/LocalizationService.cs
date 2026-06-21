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

    public async Task LoadAsync(int websiteId, int languageId, CancellationToken ct = default)
    {
        var translations = await _context.Translations
            .Where(t => t.LanguageId == languageId && (t.WebsiteId == null || t.WebsiteId == websiteId))
            .Select(t => new { t.WebsiteId, t.Key, t.Value })
            .ToListAsync(ct);

        _cache.Load(translations.Select(t => (t.WebsiteId ?? 0, languageId, t.Key, t.Value)));
    }

    public string Get(string key, string? fallback = null) =>
        _cache.TryGet(0, 0, key, out var v) ? v : fallback ?? key;

    public string Get(string key, int websiteId, int languageId, string? fallback = null) =>
        _cache.TryGet(websiteId, languageId, key, out var v) ? v : fallback ?? key;

    public async Task<IReadOnlyDictionary<string, string>> GetAllAsync(int websiteId, int languageId, CancellationToken ct = default) =>
        await _context.Translations
            .Where(t => t.LanguageId == languageId && (t.WebsiteId == null || t.WebsiteId == websiteId))
            .ToDictionaryAsync(t => t.Key, t => t.Value, ct);

    public async Task SetAsync(int websiteId, int languageId, string key, string value, CancellationToken ct = default)
    {
        var existing = await _context.Translations
            .FirstOrDefaultAsync(t => t.WebsiteId == websiteId && t.LanguageId == languageId && t.Key == key, ct);

        if (existing is null)
            _context.Translations.Add(new Translation { WebsiteId = websiteId, LanguageId = languageId, Key = key, Value = value });
        else
        {
            existing.Value = value;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
        _cache.Set(websiteId, languageId, key, value);
    }
}
