using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dotnetable.Application.DTOs;

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

    /// <summary>Fetches the active navigation menu for a location (e.g. "Header", "Footer"), or null
    /// when none is configured or the API is unreachable — callers fall back to static navigation.</summary>
    public async Task<MenuDto?> GetMenuAsync(string location, string? lang = null, CancellationToken ct = default)
    {
        var path = $"api/menu/{Uri.EscapeDataString(location)}";
        if (!string.IsNullOrWhiteSpace(lang))
            path += $"?lang={Uri.EscapeDataString(lang)}";

        try
        {
            // 204 No Content (no menu assigned) deserializes to null, which is exactly what we want.
            return await _http.GetFromJsonAsync<MenuDto>(path, ct);
        }
        catch (HttpRequestException) { return null; }
        catch (NotSupportedException) { return null; }
    }

    // ── Content (posts, pages, categories, redirects) ───────────────

    /// <summary>Published posts (paged), optionally filtered by post type / category / tag slug.</summary>
    public async Task<PagedResult<PostSummaryDto>> GetPostsAsync(
        string? type = null, string? category = null, string? tag = null,
        int page = 1, int pageSize = 12, string? lang = null, CancellationToken ct = default)
    {
        var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(type)) query.Add($"type={Uri.EscapeDataString(type)}");
        if (!string.IsNullOrWhiteSpace(category)) query.Add($"category={Uri.EscapeDataString(category)}");
        if (!string.IsNullOrWhiteSpace(tag)) query.Add($"tag={Uri.EscapeDataString(tag)}");
        if (!string.IsNullOrWhiteSpace(lang)) query.Add($"lang={Uri.EscapeDataString(lang)}");

        try
        {
            return await _http.GetFromJsonAsync<PagedResult<PostSummaryDto>>($"api/posts?{string.Join('&', query)}", ct)
                ?? new PagedResult<PostSummaryDto>();
        }
        catch (HttpRequestException) { return new PagedResult<PostSummaryDto>(); }
        catch (NotSupportedException) { return new PagedResult<PostSummaryDto>(); }
    }

    /// <summary>A single published post by slug, or null when not found / unreachable.</summary>
    public async Task<PostDetailDto?> GetPostAsync(string slug, string? lang = null, CancellationToken ct = default)
    {
        var path = $"api/posts/{Uri.EscapeDataString(slug)}";
        if (!string.IsNullOrWhiteSpace(lang)) path += $"?lang={Uri.EscapeDataString(lang)}";
        try { return await _http.GetFromJsonAsync<PostDetailDto>(path, ct); }
        catch (HttpRequestException) { return null; }
        catch (NotSupportedException) { return null; }
    }

    /// <summary>Featured published posts.</summary>
    public async Task<IReadOnlyList<PostSummaryDto>> GetFeaturedPostsAsync(int take = 4, string? lang = null, CancellationToken ct = default)
    {
        var path = $"api/posts/featured?take={take}";
        if (!string.IsNullOrWhiteSpace(lang)) path += $"&lang={Uri.EscapeDataString(lang)}";
        try { return await _http.GetFromJsonAsync<List<PostSummaryDto>>(path, ct) ?? new(); }
        catch (HttpRequestException) { return Array.Empty<PostSummaryDto>(); }
        catch (NotSupportedException) { return Array.Empty<PostSummaryDto>(); }
    }

    /// <summary>Active category tree (optionally for a post type).</summary>
    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(int? postTypeId = null, string? lang = null, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (postTypeId is int id) query.Add($"postTypeId={id}");
        if (!string.IsNullOrWhiteSpace(lang)) query.Add($"lang={Uri.EscapeDataString(lang)}");
        var path = "api/categories" + (query.Count > 0 ? $"?{string.Join('&', query)}" : "");
        try { return await _http.GetFromJsonAsync<List<CategoryDto>>(path, ct) ?? new(); }
        catch (HttpRequestException) { return Array.Empty<CategoryDto>(); }
        catch (NotSupportedException) { return Array.Empty<CategoryDto>(); }
    }

    /// <summary>A single active CMS page by slug, or null.</summary>
    public async Task<PageDto?> GetPageAsync(string slug, string? lang = null, CancellationToken ct = default)
    {
        var path = $"api/pages/{Uri.EscapeDataString(slug)}";
        if (!string.IsNullOrWhiteSpace(lang)) path += $"?lang={Uri.EscapeDataString(lang)}";
        try { return await _http.GetFromJsonAsync<PageDto>(path, ct); }
        catch (HttpRequestException) { return null; }
        catch (NotSupportedException) { return null; }
    }

    /// <summary>Resolves a request path to a redirect target, or null when no rule matches / unreachable.</summary>
    public async Task<RedirectResultDto?> ResolveRedirectAsync(string path, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"api/redirects/resolve?path={Uri.EscapeDataString(path)}", ct);
            if (response.StatusCode == HttpStatusCode.NoContent || !response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<RedirectResultDto>(cancellationToken: ct);
        }
        catch (HttpRequestException) { return null; }
        catch (NotSupportedException) { return null; }
    }

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
