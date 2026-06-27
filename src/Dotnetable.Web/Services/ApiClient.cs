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

    /// <summary>Outcome of a registration attempt: the API's HTTP success flag and its message.</summary>
    public sealed record ApiResult(bool Success, string? Message);

    /// <summary>Registers a new customer. The website is resolved by the API from the configured website key header.</summary>
    public async Task<ApiResult> RegisterAsync(object payload, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", payload, ct);
        string? message = null;
        try
        {
            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(cancellationToken: ct);
            if (body is not null && body.TryGetValue("message", out var m))
                message = m?.ToString();
        }
        catch { /* non-JSON body — fall back to a generic message below */ }

        return new ApiResult(response.IsSuccessStatusCode, message);
    }
}
