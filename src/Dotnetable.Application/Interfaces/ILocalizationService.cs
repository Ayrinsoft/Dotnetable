using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Localization key/value store. <c>websiteId = null</c> is the admin panel string bag
/// (Initial Data → Admin Translations). A non-null id is that website's storefront keys only
/// (Website → Translations), including master site 1 — never mixed with admin UI keys.
/// </summary>
public interface ILocalizationService
{
    /// <param name="websiteId">Null = admin panel catalog; otherwise a specific website.</param>
    Task LoadAsync(int? websiteId, string languageCode, CancellationToken ct = default);
    string Get(string key, string? fallback = null);
    string Get(int? websiteId, string languageCode, string key, string? fallback = null);
    Task<IReadOnlyDictionary<string, string>> GetAllAsync(int? websiteId, string languageCode, CancellationToken ct = default);

    /// <summary>Server-side paged/sorted/searched translation key-value rows for a language.</summary>
    Task<PagedResult<TranslationEntry>> GetPagedAsync(int? websiteId, string languageCode, GridQuery query, CancellationToken ct = default);
    Task SetAsync(int? websiteId, string languageCode, string key, string value, CancellationToken ct = default);

    /// <summary>Updates the key's <c>DefaultValue</c> fallback (used when a language has no value yet).
    /// Creates the key if missing. Does not change per-language values.</summary>
    Task SetDefaultValueAsync(int? websiteId, string key, string defaultValue, CancellationToken ct = default);

    /// <summary>Builds an .xlsx workbook — columns Key,Default,Value — for a language, ready to hand
    /// to a translator and re-import. When <paramref name="untranslatedOnly"/> is true, only keys that
    /// have no value yet for this language are included (so existing translations are never re-exported
    /// and can't be accidentally overwritten on re-import).</summary>
    Task<byte[]> ExportExcelAsync(int? websiteId, string languageCode, bool untranslatedOnly = false, CancellationToken ct = default);

    /// <summary>Applies a translated .xlsx (produced by <see cref="ExportExcelAsync"/>) back into the language.</summary>
    Task<LocalizationImportResult> ImportExcelAsync(int? websiteId, string languageCode, Stream excel, CancellationToken ct = default);
}
