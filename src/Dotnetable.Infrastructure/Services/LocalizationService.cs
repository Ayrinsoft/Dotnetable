using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
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

    /// <summary>Cache uses 0 for admin (null WebsiteID).</summary>
    private static int CacheId(int? websiteId) => websiteId ?? 0;

    public async Task LoadAsync(int? websiteId, string languageCode, CancellationToken ct = default)
    {
        var entries = await QueryFor(websiteId, languageCode).ToListAsync(ct);
        var cid = CacheId(websiteId);
        _cache.Load(entries.Select(e => (cid, languageCode, e.Key, e.Value)));
    }

    private IQueryable<KeyValue> QueryFor(int? websiteId, string languageCode) =>
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

    public string Get(int? websiteId, string languageCode, string key, string? fallback = null) =>
        _cache.TryGet(CacheId(websiteId), languageCode, key, out var v) ? v : fallback ?? key;

    public async Task<IReadOnlyDictionary<string, string>> GetAllAsync(int? websiteId, string languageCode, CancellationToken ct = default)
    {
        var entries = await QueryFor(websiteId, languageCode).ToListAsync(ct);
        return entries.ToDictionary(e => e.Key, e => e.Value);
    }

    public async Task<PagedResult<TranslationEntry>> GetPagedAsync(int? websiteId, string languageCode, GridQuery query, CancellationToken ct = default)
    {
        var projected = _context.LocalizationKeys
            .Where(k => k.WebsiteID == websiteId)
            .Select(k => new
            {
                Key = k.ItemKey,
                Value = k.LocalizationValues
                    .Where(v => v.LanguageCode == languageCode)
                    .Select(v => v.ItemValue)
                    .FirstOrDefault() ?? k.DefaultValue
            });

        if (query.GetSearch("Key") is string key)
            projected = projected.Where(e => e.Key.Contains(key));
        if (query.GetSearch("Value") is string value)
            projected = projected.Where(e => e.Value.Contains(value));

        var total = await projected.CountAsync(ct);
        var items = await projected
            .ApplyOrderBy(query.OrderBy, nameof(TranslationEntry.Key))
            .Skip(query.Skip).Take(query.Take)
            .Select(e => new TranslationEntry(e.Key, e.Value))
            .ToListAsync(ct);

        return new PagedResult<TranslationEntry> { Items = items, TotalCount = total };
    }

    public async Task SetAsync(int? websiteId, string languageCode, string key, string value, CancellationToken ct = default)
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
        _cache.Set(CacheId(websiteId), languageCode, key, value);
    }

    public async Task SetDefaultValueAsync(int? websiteId, string key, string defaultValue, CancellationToken ct = default)
    {
        var localizationKey = await _context.LocalizationKeys
            .FirstOrDefaultAsync(k => k.WebsiteID == websiteId && k.ItemKey == key, ct);

        if (localizationKey is null)
        {
            localizationKey = new LocalizationKey
            {
                WebsiteID = websiteId,
                ItemKey = key,
                DefaultValue = defaultValue,
            };
            _context.LocalizationKeys.Add(localizationKey);
        }
        else
        {
            localizationKey.DefaultValue = defaultValue;
        }

        await _context.SaveChangesAsync(ct);
    }

    private const int MaxKeyLength = 72;
    private const int MaxValueLength = 2000;

    public async Task<byte[]> ExportExcelAsync(int? websiteId, string languageCode, bool untranslatedOnly = false, CancellationToken ct = default)
    {
        var query = _context.LocalizationKeys.Where(k => k.WebsiteID == websiteId);

        if (untranslatedOnly)
            query = query.Where(k => !k.LocalizationValues.Any(v => v.LanguageCode == languageCode));

        var rows = await query
            .OrderBy(k => k.ItemKey)
            .Select(k => new
            {
                k.ItemKey,
                k.DefaultValue,
                Value = k.LocalizationValues
                    .Where(v => v.LanguageCode == languageCode)
                    .Select(v => v.ItemValue)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        return ExcelWorkbook.Write(
            "Translations",
            ["Key", "Default", "Value"],
            rows.Select(r => (IReadOnlyList<object?>)[r.ItemKey, r.DefaultValue, r.Value ?? r.DefaultValue]));
    }

    public async Task<LocalizationImportResult> ImportExcelAsync(int? websiteId, string languageCode, Stream excel, CancellationToken ct = default)
    {
        var rows = ExcelWorkbook.Read(excel);

        var keys = await _context.LocalizationKeys
            .Where(k => k.WebsiteID == websiteId)
            .Include(k => k.LocalizationValues)
            .ToDictionaryAsync(k => k.ItemKey, ct);

        int added = 0, updated = 0, unchanged = 0, skipped = 0;
        var errors = new List<string>();

        int start = rows.Count > 0 && rows[0].Length > 0 &&
                    rows[0][0].Trim().Equals("Key", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

        for (int i = start; i < rows.Count; i++)
        {
            var row = rows[i];
            int line = i + 1;

            if (row.Length == 0 || (row.Length == 1 && string.IsNullOrWhiteSpace(row[0])))
                continue;

            string key = row[0].Trim();
            string value;
            string? defaultValue = null;

            if (row.Length >= 3) { defaultValue = row[1]; value = row[2]; }
            else if (row.Length == 2) { value = row[1]; }
            else { errors.Add($"Row {line}: expected Key,Default,Value columns."); skipped++; continue; }

            if (string.IsNullOrWhiteSpace(key)) { skipped++; continue; }
            if (key.Length > MaxKeyLength) { errors.Add($"Row {line}: key exceeds {MaxKeyLength} characters."); skipped++; continue; }
            if (value.Length > MaxValueLength)
            {
                errors.Add($"Row {line}: value truncated to {MaxValueLength} characters.");
                value = value[..MaxValueLength];
            }

            if (!keys.TryGetValue(key, out var localizationKey))
            {
                localizationKey = new LocalizationKey
                {
                    WebsiteID = websiteId,
                    ItemKey = key,
                    DefaultValue = string.IsNullOrEmpty(defaultValue) ? value : defaultValue[..Math.Min(defaultValue.Length, MaxValueLength)],
                };
                localizationKey.LocalizationValues.Add(new LocalizationValue { LanguageCode = languageCode, ItemValue = value });
                _context.LocalizationKeys.Add(localizationKey);
                keys[key] = localizationKey;
                added++;
                continue;
            }

            var localizedValue = localizationKey.LocalizationValues.FirstOrDefault(v => v.LanguageCode == languageCode);
            if (localizedValue is null)
            {
                localizationKey.LocalizationValues.Add(new LocalizationValue { LanguageCode = languageCode, ItemValue = value });
                updated++;
            }
            else if (localizedValue.ItemValue != value)
            {
                localizedValue.ItemValue = value;
                updated++;
            }
            else unchanged++;
        }

        if (added > 0 || updated > 0)
        {
            await _context.SaveChangesAsync(ct);
            await LoadAsync(websiteId, languageCode, ct);
        }

        return new LocalizationImportResult(added, updated, unchanged, skipped, errors);
    }
}
