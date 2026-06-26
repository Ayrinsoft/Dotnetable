using System.Net.Http.Json;

namespace Dotnetable.Web.Services;

/// <summary>A JWT access token issued by the API for a website customer.</summary>
public sealed record LoginResult(string AccessToken, DateTime ExpiresAtUtc, string TokenType);

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<T?> GetAsync<T>(string path, CancellationToken ct = default) =>
        await _http.GetFromJsonAsync<T>(path, ct);

    public async Task<IReadOnlyDictionary<string, string>> GetTranslationsAsync(string languageCode, CancellationToken ct = default) =>
        await _http.GetFromJsonAsync<Dictionary<string, string>>($"api/localization/{languageCode}", ct)
        ?? new Dictionary<string, string>();

    /// <summary>Authenticates a customer against the API. Returns the issued token, or null on bad credentials.</summary>
    public async Task<LoginResult?> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new { username, password }, ct);
        if (!response.IsSuccessStatusCode)
            return null;
        return await response.Content.ReadFromJsonAsync<LoginResult>(cancellationToken: ct);
    }
}
