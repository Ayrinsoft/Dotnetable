using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

public interface ILocalizationService
{
    Task LoadAsync(int websiteId, string languageCode, CancellationToken ct = default);
    string Get(string key, string? fallback = null);
    string Get(int websiteId, string languageCode, string key, string? fallback = null);
    Task<IReadOnlyDictionary<string, string>> GetAllAsync(int websiteId, string languageCode, CancellationToken ct = default);

    /// <summary>Server-side paged/sorted/searched translation key-value rows for a language.</summary>
    Task<PagedResult<TranslationEntry>> GetPagedAsync(int websiteId, string languageCode, GridQuery query, CancellationToken ct = default);
    Task SetAsync(int websiteId, string languageCode, string key, string value, CancellationToken ct = default);

    /// <summary>Builds a UTF-8 (BOM) CSV — columns Key,Default,Value — for a language, ready to hand
    /// to a translator or Google Translate and re-import. When <paramref name="untranslatedOnly"/> is
    /// true, only keys that have no value yet for this language are included (so existing translations
    /// are never re-exported and can't be accidentally overwritten on re-import).</summary>
    Task<byte[]> ExportCsvAsync(int websiteId, string languageCode, bool untranslatedOnly = false, CancellationToken ct = default);

    /// <summary>Applies a translated CSV (produced by <see cref="ExportCsvAsync"/>) back into the language.</summary>
    Task<LocalizationImportResult> ImportCsvAsync(int websiteId, string languageCode, Stream csv, CancellationToken ct = default);
}
