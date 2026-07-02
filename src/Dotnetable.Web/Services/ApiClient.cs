using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Dotnetable.Web.Services;

/// <summary>A JWT access token issued by the API for a website customer.</summary>
public sealed record LoginResult(string AccessToken, DateTime ExpiresAtUtc, string TokenType);

/// <summary>
/// Result of a customer auth API call: the HTTP status, a parsed <c>message</c>, any extra string
/// fields the endpoint returned (channel, identifier, status, …), and the issued token when present.
/// </summary>
public sealed record AuthApiResult(
    bool Ok,
    HttpStatusCode Status,
    string? Message,
    IReadOnlyDictionary<string, string> Fields,
    LoginResult? Token);

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

    public Task<AuthApiResult> LoginAsync(string identifier, string password, CancellationToken ct = default) =>
        PostAsync("api/auth/login", new { identifier, password }, ct);

    public Task<AuthApiResult> RegisterAsync(object payload, CancellationToken ct = default) =>
        PostAsync("api/auth/register", payload, ct);

    public Task<AuthApiResult> VerifyOtpAsync(string identifier, string code, CancellationToken ct = default) =>
        PostAsync("api/auth/verify-otp", new { identifier, code }, ct);

    public Task<AuthApiResult> ResendOtpAsync(string identifier, CancellationToken ct = default) =>
        PostAsync("api/auth/resend-otp", new { identifier }, ct);

    public Task<AuthApiResult> ForgotPasswordAsync(string identifier, CancellationToken ct = default) =>
        PostAsync("api/auth/forgot-password", new { identifier }, ct);

    public Task<AuthApiResult> ResetPasswordAsync(string identifier, string code, string newPassword, CancellationToken ct = default) =>
        PostAsync("api/auth/reset-password", new { identifier, code, newPassword }, ct);

    /// <summary>POSTs JSON and normalizes the response into an <see cref="AuthApiResult"/>.</summary>
    private async Task<AuthApiResult> PostAsync(string path, object payload, CancellationToken ct)
    {
        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsJsonAsync(path, payload, ct);
        }
        catch (HttpRequestException)
        {
            return new AuthApiResult(false, HttpStatusCode.ServiceUnavailable,
                "Service is unavailable. Please try again later.",
                new Dictionary<string, string>(), null);
        }

        string? message = null;
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        LoginResult? token = null;

        try
        {
            using var doc = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: ct);
            if (doc is not null && doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.String)
                        fields[prop.Name] = prop.Value.GetString()!;
                }

                fields.TryGetValue("message", out message);

                if (fields.TryGetValue("accessToken", out var accessToken) &&
                    doc.RootElement.TryGetProperty("expiresAtUtc", out var exp) &&
                    exp.TryGetDateTime(out var expiresAt))
                {
                    token = new LoginResult(accessToken, expiresAt, "Bearer");
                }
            }
        }
        catch { /* non-JSON body — fall back to the generic message below */ }

        return new AuthApiResult(response.IsSuccessStatusCode, response.StatusCode, message, fields, token);
    }
}
