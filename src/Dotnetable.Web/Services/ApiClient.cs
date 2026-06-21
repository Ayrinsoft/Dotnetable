using System.Net.Http.Json;

namespace Dotnetable.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<T?> GetAsync<T>(string path, CancellationToken ct = default) =>
        await _http.GetFromJsonAsync<T>(path, ct);

    public async Task<IReadOnlyDictionary<string, string>> GetTranslationsAsync(int languageId, CancellationToken ct = default) =>
        await _http.GetFromJsonAsync<Dictionary<string, string>>($"api/localization/{languageId}", ct)
        ?? new Dictionary<string, string>();
}
