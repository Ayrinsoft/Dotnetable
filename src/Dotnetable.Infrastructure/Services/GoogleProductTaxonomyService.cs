using System.Text.RegularExpressions;
using Dotnetable.Application;
using Dotnetable.Application.Catalog;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class GoogleProductTaxonomyService : IGoogleProductTaxonomyService
{
    public const string DefaultUrl = "https://www.google.com/basepages/producttype/taxonomy-with-ids.en-US.txt";

    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IHttpClientFactory _http;
    private readonly IExcelExporterRegistry _excel;

    public GoogleProductTaxonomyService(
        IDbContextFactory<AppDbContext> contextFactory,
        IHttpClientFactory http,
        IExcelExporterRegistry excel)
    {
        _contextFactory = contextFactory;
        _http = http;
        _excel = excel;
    }

    public async Task<GoogleTaxonomySyncResult> SyncAsync(string? url = null, CancellationToken ct = default)
    {
        var target = string.IsNullOrWhiteSpace(url) ? DefaultUrl : url.Trim();
        if (!IsAllowedGoogleTaxonomyUrl(target))
            throw new InvalidOperationException("Only Google product taxonomy files on https://www.google.com/basepages/producttype/ are allowed.");

        var client = _http.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(90);
        string text;
        try
        {
            text = await client.GetStringAsync(target, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("The Google taxonomy file could not be downloaded.", ex);
        }

        var rows = GoogleProductTaxonomyParser.Parse(text);
        if (rows.Count == 0)
            throw new InvalidOperationException("The file did not contain any taxonomy rows.");

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var existing = await context.GoogleProductTaxonomyEntries.ToDictionaryAsync(e => e.GoogleId, ct);
        var now = DateTime.UtcNow;
        var seen = new HashSet<int>(rows.Count);
        var added = 0;
        var updated = 0;

        foreach (var row in rows)
        {
            seen.Add(row.GoogleId);
            if (!existing.TryGetValue(row.GoogleId, out var entry))
            {
                context.GoogleProductTaxonomyEntries.Add(new GoogleProductTaxonomyEntry
                {
                    GoogleId = row.GoogleId,
                    ParentGoogleId = row.ParentGoogleId,
                    Name = Trim(row.Name, 300),
                    FullPath = Trim(row.FullPath, 1000),
                    Depth = row.Depth,
                    LastSeenUtc = now,
                    AbsentFromLatest = false,
                });
                added++;
            }
            else
            {
                entry.ParentGoogleId = row.ParentGoogleId;
                entry.Name = Trim(row.Name, 300);
                entry.FullPath = Trim(row.FullPath, 1000);
                entry.Depth = row.Depth;
                entry.LastSeenUtc = now;
                entry.AbsentFromLatest = false;
                updated++;
            }
        }

        var markedAbsent = 0;
        foreach (var entry in existing.Values)
        {
            if (seen.Contains(entry.GoogleId) || entry.AbsentFromLatest) continue;
            entry.AbsentFromLatest = true;
            markedAbsent++;
        }

        await context.SaveChangesAsync(ct);

        var imported = context.ProductCategories.AsNoTracking()
            .Where(c => c.WebsiteID == AppConstants.MasterWebsiteId && c.GoogleProductCategoryId != null)
            .Select(c => c.GoogleProductCategoryId!.Value);
        var notYet = await context.GoogleProductTaxonomyEntries.AsNoTracking()
            .CountAsync(e => !e.AbsentFromLatest && !imported.Contains(e.GoogleId), ct);

        return new GoogleTaxonomySyncResult
        {
            Parsed = rows.Count,
            Added = added,
            Updated = updated,
            MarkedAbsent = markedAbsent,
            NotYetImported = notYet,
        };
    }

    public async Task<PagedResult<GoogleTaxonomyDiffItem>> GetDiffAsync(bool onlyMissing, int skip, int take, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var imported = context.ProductCategories.AsNoTracking()
            .Where(c => c.WebsiteID == AppConstants.MasterWebsiteId && c.GoogleProductCategoryId != null)
            .Select(c => c.GoogleProductCategoryId!.Value);

        var q = context.GoogleProductTaxonomyEntries.AsNoTracking().AsQueryable();
        if (onlyMissing)
            q = q.Where(e => !e.AbsentFromLatest && !imported.Contains(e.GoogleId));

        var total = await q.CountAsync(ct);
        var pageSize = take < 1 ? 50 : Math.Min(take, 200);
        var items = await q.OrderBy(e => e.FullPath)
            .Skip(skip < 0 ? 0 : skip)
            .Take(pageSize)
            .Select(e => new GoogleTaxonomyDiffItem
            {
                GoogleId = e.GoogleId,
                Name = e.Name,
                FullPath = e.FullPath,
                Depth = e.Depth,
                AbsentFromLatest = e.AbsentFromLatest,
            })
            .ToListAsync(ct);

        return new PagedResult<GoogleTaxonomyDiffItem> { Items = items, TotalCount = total };
    }

    public async Task<GoogleTaxonomyImportResult> ImportMissingAsync(IReadOnlyList<int>? googleIds = null, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var wantedQuery = context.GoogleProductTaxonomyEntries.AsNoTracking().Where(e => !e.AbsentFromLatest);
        if (googleIds is { Count: > 0 })
        {
            var ids = googleIds.Distinct().ToList();
            wantedQuery = wantedQuery.Where(e => ids.Contains(e.GoogleId));
        }

        var rows = await wantedQuery
            .OrderBy(e => e.Depth).ThenBy(e => e.GoogleId)
            .Select(e => new { e.GoogleId, e.ParentGoogleId, e.Name, e.Depth })
            .ToListAsync(ct);

        var categories = await context.ProductCategories
            .Where(c => c.WebsiteID == AppConstants.MasterWebsiteId && c.GoogleProductCategoryId != null)
            .ToListAsync(ct);
        var byGoogle = categories.ToDictionary(c => c.GoogleProductCategoryId!.Value);

        var usedSlugs = (await context.ProductCategories.AsNoTracking()
                .Where(c => c.WebsiteID == AppConstants.MasterWebsiteId)
                .Select(c => c.Slug)
                .ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var result = new GoogleTaxonomyImportResult();
        foreach (var group in rows.GroupBy(r => r.Depth).OrderBy(g => g.Key))
        {
            var batch = new List<ProductCategory>();
            foreach (var row in group)
            {
                if (byGoogle.ContainsKey(row.GoogleId))
                {
                    result.AlreadyPresent++;
                    continue;
                }

                int? parentId = null;
                if (row.ParentGoogleId is int parentGoogle)
                {
                    if (!byGoogle.TryGetValue(parentGoogle, out var parent) || parent.ProductCategoryID == 0)
                    {
                        result.Skipped++;
                        continue;
                    }
                    parentId = parent.ProductCategoryID;
                }

                batch.Add(new ProductCategory
                {
                    WebsiteID = AppConstants.MasterWebsiteId,
                    ParentCategoryID = parentId,
                    Name = Trim(row.Name, 200),
                    Slug = UniqueSlug(usedSlugs, row.Name, row.GoogleId),
                    SortOrder = 0,
                    IsActive = true,
                    GoogleProductCategoryId = row.GoogleId,
                });
            }

            if (batch.Count == 0) continue;
            context.ProductCategories.AddRange(batch);
            await context.SaveChangesAsync(ct);
            foreach (var category in batch)
            {
                byGoogle[category.GoogleProductCategoryId!.Value] = category;
                context.Entry(category).State = EntityState.Detached;
                result.Created++;
            }
        }

        return result;
    }

    public async Task<byte[]> ExportTranslationsAsync(string languageCode, CancellationToken ct = default)
    {
        var language = NormalizeLanguage(languageCode);
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var categories = await context.ProductCategories.AsNoTracking()
            .Where(c => c.WebsiteID == AppConstants.MasterWebsiteId)
            .Select(c => new CategoryNode
            {
                ProductCategoryID = c.ProductCategoryID,
                ParentCategoryID = c.ParentCategoryID,
                Name = c.Name,
                GoogleProductCategoryId = c.GoogleProductCategoryId,
            })
            .ToListAsync(ct);
        var ids = categories.Select(c => c.ProductCategoryID).ToList();
        var translations = await context.ProductCategoryTranslations.AsNoTracking()
            .Where(t => t.LanguageCode == language && ids.Contains(t.ProductCategoryID))
            .ToDictionaryAsync(t => t.ProductCategoryID, ct);

        var byId = categories.ToDictionary(c => c.ProductCategoryID);
        var rows = new List<IReadOnlyList<object?>>(categories.Count);
        foreach (var category in categories.OrderBy(c => c.Name))
        {
            translations.TryGetValue(category.ProductCategoryID, out var translation);
            rows.Add(
            [
                category.ProductCategoryID,
                category.GoogleProductCategoryId,
                language,
                BuildPath(category.ProductCategoryID, byId),
                category.Name,
                translation?.Name ?? "",
                translation?.Slug ?? "",
            ]);
        }

        return _excel.Write(ProductCategoryTranslationSheet.SheetName, ProductCategoryTranslationSheet.Headers, rows);
    }

    public async Task<ProductCategoryTranslationImportResult> ImportTranslationsAsync(Stream workbook, CancellationToken ct = default)
    {
        var table = _excel.Read(workbook);
        var result = new ProductCategoryTranslationImportResult();
        if (table.Count == 0)
        {
            result.Errors.Add("The workbook is empty.");
            return result;
        }

        var header = table[0];
        var columns = MapColumns(header);
        if (columns.ProductCategoryId < 0 || columns.LanguageCode < 0 || columns.TranslatedName < 0)
        {
            result.Errors.Add("The header row must keep ProductCategoryID, LanguageCode and TranslatedName.");
            return result;
        }

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var defaultLanguage = await context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == AppConstants.MasterWebsiteId)
            .Select(w => w.DefaultLanguageCode)
            .FirstOrDefaultAsync(ct);

        var ids = new List<int>();
        for (var i = 1; i < table.Count; i++)
        {
            if (int.TryParse(Cell(table[i], columns.ProductCategoryId), out var id))
                ids.Add(id);
        }

        var categories = await context.ProductCategories
            .Where(c => c.WebsiteID == AppConstants.MasterWebsiteId && ids.Contains(c.ProductCategoryID))
            .ToDictionaryAsync(c => c.ProductCategoryID, ct);
        var translations = await context.ProductCategoryTranslations
            .Where(t => ids.Contains(t.ProductCategoryID))
            .ToListAsync(ct);

        for (var i = 1; i < table.Count; i++)
        {
            var row = table[i];
            var line = i + 1;
            if (!int.TryParse(Cell(row, columns.ProductCategoryId), out var id))
            {
                result.Skipped++;
                continue;
            }

            var translated = Cell(row, columns.TranslatedName);
            if (string.IsNullOrWhiteSpace(translated))
            {
                result.Skipped++;
                continue;
            }

            if (!categories.TryGetValue(id, out var category))
            {
                result.Errors.Add($"Row {line}: category {id} is not on website 1.");
                result.Skipped++;
                continue;
            }

            var language = NormalizeLanguage(Cell(row, columns.LanguageCode));
            if (language.Length != 2)
            {
                result.Errors.Add($"Row {line}: LanguageCode must be two letters.");
                result.Skipped++;
                continue;
            }

            var slugCell = columns.TranslatedSlug < 0 ? "" : Cell(row, columns.TranslatedSlug);
            var slug = string.IsNullOrWhiteSpace(slugCell) ? Slugify(translated) : Slugify(slugCell);
            var name = Trim(translated, 200);

            if (string.Equals(language, defaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                category.Name = name;
                category.Slug = slug;
            }

            var current = translations.FirstOrDefault(t =>
                t.ProductCategoryID == id && string.Equals(t.LanguageCode, language, StringComparison.OrdinalIgnoreCase));
            if (current is null)
            {
                var created = new ProductCategoryTranslation
                {
                    ProductCategoryID = id,
                    LanguageCode = language,
                    Name = name,
                    Slug = slug,
                };
                context.ProductCategoryTranslations.Add(created);
                translations.Add(created);
            }
            else
            {
                current.Name = name;
                current.Slug = slug;
            }

            result.Updated++;
        }

        await context.SaveChangesAsync(ct);
        return result;
    }

    private static string BuildPath(int id, Dictionary<int, CategoryNode> byId)
    {
        var parts = new List<string>();
        var guard = 0;
        var currentId = (int?)id;
        while (currentId is int cid && byId.TryGetValue(cid, out var node) && guard++ < 16)
        {
            parts.Insert(0, node.Name);
            currentId = node.ParentCategoryID;
        }
        return string.Join(" > ", parts);
    }

    private static (int ProductCategoryId, int LanguageCode, int TranslatedName, int TranslatedSlug) MapColumns(string[] header)
    {
        int Index(string name)
        {
            for (var i = 0; i < header.Length; i++)
            {
                if (string.Equals(header[i]?.Trim(), name, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        return (
            Index(ProductCategoryTranslationSheet.ProductCategoryId),
            Index(ProductCategoryTranslationSheet.LanguageCode),
            Index(ProductCategoryTranslationSheet.TranslatedName),
            Index(ProductCategoryTranslationSheet.TranslatedSlug));
    }

    private static string Cell(string[] row, int index) =>
        index < 0 || index >= row.Length ? "" : (row[index] ?? "").Trim();

    private static string NormalizeLanguage(string? code)
    {
        var value = (code ?? "").Trim().ToLowerInvariant();
        return value.Length >= 2 ? value[..2] : value;
    }

    internal static string Slugify(string value)
    {
        var s = value.Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"\s+", "-");
        s = Regex.Replace(s, @"[^a-z0-9-]+", "");
        s = Regex.Replace(s, "-{2,}", "-").Trim('-');
        if (string.IsNullOrEmpty(s)) s = "category";
        return s.Length <= 200 ? s : s[..200].Trim('-');
    }

    private static string UniqueSlug(HashSet<string> used, string name, int googleId)
    {
        var slug = Slugify(name);
        if (used.Add(slug)) return slug;
        var alt = $"{slug}-{googleId}";
        if (alt.Length > 200) alt = alt[..200].Trim('-');
        used.Add(alt);
        return alt;
    }

    private static string Trim(string value, int max)
    {
        var text = value.Trim();
        return text.Length <= max ? text : text[..max];
    }

    internal static bool IsAllowedGoogleTaxonomyUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
        if (uri.Scheme != Uri.UriSchemeHttps) return false;
        if (!uri.Host.Equals("www.google.com", StringComparison.OrdinalIgnoreCase)) return false;
        return uri.AbsolutePath.StartsWith("/basepages/producttype/", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class CategoryNode
    {
        public int ProductCategoryID { get; set; }
        public int? ParentCategoryID { get; set; }
        public string Name { get; set; } = "";
        public int? GoogleProductCategoryId { get; set; }
    }
}
