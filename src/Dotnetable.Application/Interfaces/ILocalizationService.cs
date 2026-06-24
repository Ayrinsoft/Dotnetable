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
}
