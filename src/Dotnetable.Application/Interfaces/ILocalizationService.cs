namespace Dotnetable.Application.Interfaces;

public interface ILocalizationService
{
    Task LoadAsync(int websiteId, int languageId, CancellationToken ct = default);
    string Get(string key, string? fallback = null);
    string Get(string key, int websiteId, int languageId, string? fallback = null);
    Task<IReadOnlyDictionary<string, string>> GetAllAsync(int websiteId, int languageId, CancellationToken ct = default);
    Task SetAsync(int websiteId, int languageId, string key, string value, CancellationToken ct = default);
}
