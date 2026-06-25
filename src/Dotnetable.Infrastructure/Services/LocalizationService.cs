using System.Text;
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

    public async Task<PagedResult<TranslationEntry>> GetPagedAsync(int websiteId, string languageCode, GridQuery query, CancellationToken ct = default)
    {
        var projected = _context.LocalizationKeys
            .Where(k => k.WebsiteID == websiteId)
            .Select(k => new TranslationEntry(
                k.ItemKey,
                k.LocalizationValues
                    .Where(v => v.LanguageCode == languageCode)
                    .Select(v => v.ItemValue)
                    .FirstOrDefault() ?? k.DefaultValue));

        if (query.GetSearch("Key") is string key)
            projected = projected.Where(e => e.Key.Contains(key));
        if (query.GetSearch("Value") is string value)
            projected = projected.Where(e => e.Value.Contains(value));

        var total = await projected.CountAsync(ct);
        var items = await projected
            .ApplyOrderBy(query.OrderBy, nameof(TranslationEntry.Key))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<TranslationEntry> { Items = items, TotalCount = total };
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

    // Column lengths mirror the LocalizationKey/LocalizationValue tables.
    private const int MaxKeyLength = 72;
    private const int MaxValueLength = 2000;

    public async Task<byte[]> ExportCsvAsync(int websiteId, string languageCode, bool untranslatedOnly = false, CancellationToken ct = default)
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

        var sb = new StringBuilder();
        sb.Append("Key,Default,Value\r\n");
        foreach (var r in rows)
            sb.Append(Csv.Escape(r.ItemKey)).Append(',')
              .Append(Csv.Escape(r.DefaultValue)).Append(',')
              .Append(Csv.Escape(r.Value ?? r.DefaultValue)).Append("\r\n");

        // Prepend a UTF-8 BOM so Excel opens Persian/Arabic text correctly.
        var body = new UTF8Encoding(false).GetBytes(sb.ToString());
        return [0xEF, 0xBB, 0xBF, .. body];
    }

    public async Task<LocalizationImportResult> ImportCsvAsync(int websiteId, string languageCode, Stream csv, CancellationToken ct = default)
    {
        using var reader = new StreamReader(csv, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var rows = Csv.Parse(await reader.ReadToEndAsync(ct));

        var keys = await _context.LocalizationKeys
            .Where(k => k.WebsiteID == websiteId)
            .Include(k => k.LocalizationValues)
            .ToDictionaryAsync(k => k.ItemKey, ct);

        int added = 0, updated = 0, unchanged = 0, skipped = 0;
        var errors = new List<string>();

        // Skip the header row if present.
        int start = rows.Count > 0 && rows[0].Length > 0 &&
                    rows[0][0].Trim().Equals("Key", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

        for (int i = start; i < rows.Count; i++)
        {
            var row = rows[i];
            int line = i + 1;

            // Ignore blank lines.
            if (row.Length == 0 || (row.Length == 1 && string.IsNullOrWhiteSpace(row[0])))
                continue;

            string key = row[0].Trim();
            string value;
            string? defaultValue = null;

            if (row.Length >= 3) { defaultValue = row[1]; value = row[2]; }
            else if (row.Length == 2) { value = row[1]; }
            else { errors.Add($"Line {line}: expected Key,Default,Value columns."); skipped++; continue; }

            if (string.IsNullOrWhiteSpace(key)) { skipped++; continue; }
            if (key.Length > MaxKeyLength) { errors.Add($"Line {line}: key exceeds {MaxKeyLength} characters."); skipped++; continue; }
            if (value.Length > MaxValueLength)
            {
                errors.Add($"Line {line}: value truncated to {MaxValueLength} characters.");
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
            await LoadAsync(websiteId, languageCode, ct); // refresh the in-memory cache
        }

        return new LocalizationImportResult(added, updated, unchanged, skipped, errors);
    }
}
