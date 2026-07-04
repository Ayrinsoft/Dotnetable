using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dotnetable.Application.DTOs;
using Microsoft.Extensions.Caching.Memory;

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
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _ttl;

    public ApiClient(HttpClient http, IMemoryCache cache, IConfiguration configuration)
    {
        _http = http;
        _cache = cache;
        _ttl = TimeSpan.FromSeconds(configuration.GetValue("Cache:WebTtlSeconds", 60));
    }

    /// <summary>Short-TTL read-through cache for public content reads (menu/categories/pages/posts).
    /// There's no push-invalidation from Admin/API into Web — this instance is one of potentially many
    /// per-site deployments — so the TTL alone bounds staleness after an edit.</summary>
    private Task<T?> CachedGetAsync<T>(string key, Func<Task<T?>> factory) =>
        _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _ttl;
            return await factory();
        });

    public async Task<T?> GetAsync<T>(string path, CancellationToken ct = default) =>
        await _http.GetFromJsonAsync<T>(path, ct);

    public async Task<IReadOnlyDictionary<string, string>> GetTranslationsAsync(string languageCode, CancellationToken ct = default) =>
        await _http.GetFromJsonAsync<Dictionary<string, string>>($"api/localization/{languageCode}", ct)
        ?? new Dictionary<string, string>();

    /// <summary>Fetches the active navigation menu for a location (e.g. "Header", "Footer"), or null
    /// when none is configured or the API is unreachable — callers fall back to static navigation.</summary>
    public Task<MenuDto?> GetMenuAsync(string location, string? lang = null, CancellationToken ct = default) =>
        CachedGetAsync($"menu:{location}:{lang}", async () =>
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
        });

    /// <summary>Fetches the active slideshow assigned to a placement key (e.g. "home_top"), or null
    /// when none is configured / the API is unreachable — callers simply render nothing.</summary>
    public Task<SlideshowDto?> GetSlideshowByPlacementAsync(string placementKey, CancellationToken ct = default) =>
        CachedGetAsync($"slideshow:placement:{placementKey}", async () =>
        {
            var path = $"api/slideshow/placement/{Uri.EscapeDataString(placementKey)}";
            try { return await _http.GetFromJsonAsync<SlideshowDto>(path, ct); }
            catch (HttpRequestException) { return null; }
            catch (NotSupportedException) { return null; }
        });

    /// <summary>Fetches a single active slideshow by id — used to resolve a <c>[slideshow:ID]</c>
    /// shortcode embedded inside a Post/Page body.</summary>
    public Task<SlideshowDto?> GetSlideshowByIdAsync(int slideshowId, CancellationToken ct = default) =>
        CachedGetAsync($"slideshow:id:{slideshowId}", async () =>
        {
            var path = $"api/slideshow/{slideshowId}";
            try { return await _http.GetFromJsonAsync<SlideshowDto>(path, ct); }
            catch (HttpRequestException) { return null; }
            catch (NotSupportedException) { return null; }
        });

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
        var cacheKey = $"posts:{string.Join('&', query)}";

        return await CachedGetAsync<PagedResult<PostSummaryDto>>(cacheKey, async () =>
        {
            try
            {
                return await _http.GetFromJsonAsync<PagedResult<PostSummaryDto>>($"api/posts?{string.Join('&', query)}", ct)
                    ?? new PagedResult<PostSummaryDto>();
            }
            catch (HttpRequestException) { return new PagedResult<PostSummaryDto>(); }
            catch (NotSupportedException) { return new PagedResult<PostSummaryDto>(); }
        }) ?? new PagedResult<PostSummaryDto>();
    }

    /// <summary>A single published post by slug, or null when not found / unreachable. Not cached —
    /// the API increments the post's view count as part of this read.</summary>
    public async Task<PostDetailDto?> GetPostAsync(string slug, string? lang = null, CancellationToken ct = default)
    {
        var path = $"api/posts/{Uri.EscapeDataString(slug)}";
        if (!string.IsNullOrWhiteSpace(lang)) path += $"?lang={Uri.EscapeDataString(lang)}";
        try { return await _http.GetFromJsonAsync<PostDetailDto>(path, ct); }
        catch (HttpRequestException) { return null; }
        catch (NotSupportedException) { return null; }
    }

    /// <summary>Featured published posts.</summary>
    public async Task<IReadOnlyList<PostSummaryDto>> GetFeaturedPostsAsync(int take = 4, string? lang = null, CancellationToken ct = default) =>
        await CachedGetAsync<IReadOnlyList<PostSummaryDto>>($"posts:featured:{take}:{lang}", async () =>
        {
            var path = $"api/posts/featured?take={take}";
            if (!string.IsNullOrWhiteSpace(lang)) path += $"&lang={Uri.EscapeDataString(lang)}";
            try { return await _http.GetFromJsonAsync<List<PostSummaryDto>>(path, ct) ?? new(); }
            catch (HttpRequestException) { return Array.Empty<PostSummaryDto>(); }
            catch (NotSupportedException) { return Array.Empty<PostSummaryDto>(); }
        }) ?? Array.Empty<PostSummaryDto>();

    /// <summary>Active category tree (optionally for a post type).</summary>
    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(int? postTypeId = null, string? lang = null, CancellationToken ct = default) =>
        await CachedGetAsync<IReadOnlyList<CategoryDto>>($"categories:{postTypeId}:{lang}", async () =>
        {
            var query = new List<string>();
            if (postTypeId is int id) query.Add($"postTypeId={id}");
            if (!string.IsNullOrWhiteSpace(lang)) query.Add($"lang={Uri.EscapeDataString(lang)}");
            var path = "api/categories" + (query.Count > 0 ? $"?{string.Join('&', query)}" : "");
            try { return await _http.GetFromJsonAsync<List<CategoryDto>>(path, ct) ?? new(); }
            catch (HttpRequestException) { return Array.Empty<CategoryDto>(); }
            catch (NotSupportedException) { return Array.Empty<CategoryDto>(); }
        }) ?? Array.Empty<CategoryDto>();

    /// <summary>A single active CMS page by slug, or null.</summary>
    public Task<PageDto?> GetPageAsync(string slug, string? lang = null, CancellationToken ct = default) =>
        CachedGetAsync($"page:{slug}:{lang}", async () =>
        {
            var path = $"api/pages/{Uri.EscapeDataString(slug)}";
            if (!string.IsNullOrWhiteSpace(lang)) path += $"?lang={Uri.EscapeDataString(lang)}";
            try { return await _http.GetFromJsonAsync<PageDto>(path, ct); }
            catch (HttpRequestException) { return null; }
            catch (NotSupportedException) { return null; }
        });

    /// <summary>Submits a visitor-filled contact form to the API's contact inbox.</summary>
    public Task<AuthApiResult> SubmitContactMessageAsync(ContactMessageRequest request, CancellationToken ct = default) =>
        PostAsync("api/contact", request, ct);

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

    // ── Location reference data ─────────────────────────────────────

    public async Task<IReadOnlyList<LocationOptionDto>> GetCountriesAsync(CancellationToken ct = default)
    {
        try { return await _http.GetFromJsonAsync<List<LocationOptionDto>>("api/locations/countries", ct) ?? new(); }
        catch (HttpRequestException) { return Array.Empty<LocationOptionDto>(); }
        catch (NotSupportedException) { return Array.Empty<LocationOptionDto>(); }
    }

    public async Task<IReadOnlyList<LocationOptionDto>> GetCitiesAsync(int countryId, CancellationToken ct = default)
    {
        try { return await _http.GetFromJsonAsync<List<LocationOptionDto>>($"api/locations/countries/{countryId}/cities", ct) ?? new(); }
        catch (HttpRequestException) { return Array.Empty<LocationOptionDto>(); }
        catch (NotSupportedException) { return Array.Empty<LocationOptionDto>(); }
    }

    // ── Customer addresses (requires the caller's JWT cookie — see BearerTokenHandler) ──

    /// <summary>The signed-in customer's saved addresses, or an empty list when unreachable/unauthorized.</summary>
    public async Task<IReadOnlyList<AddressDto>> GetAddressesAsync(CancellationToken ct = default)
    {
        try { return await _http.GetFromJsonAsync<List<AddressDto>>("api/addresses", ct) ?? new(); }
        catch (HttpRequestException) { return Array.Empty<AddressDto>(); }
        catch (NotSupportedException) { return Array.Empty<AddressDto>(); }
    }

    public Task<AuthApiResult> CreateAddressAsync(AddressRequest request, CancellationToken ct = default) =>
        PostAsync("api/addresses", request, ct);

    public async Task<AuthApiResult> UpdateAddressAsync(int id, AddressRequest request, CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try { response = await _http.PutAsJsonAsync($"api/addresses/{id}", request, ct); }
        catch (HttpRequestException)
        {
            return new AuthApiResult(false, HttpStatusCode.ServiceUnavailable,
                "Service is unavailable. Please try again later.", new Dictionary<string, string>(), null);
        }
        return await ToResultAsync(response, ct);
    }

    public async Task<AuthApiResult> DeleteAddressAsync(int id, CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try { response = await _http.DeleteAsync($"api/addresses/{id}", ct); }
        catch (HttpRequestException)
        {
            return new AuthApiResult(false, HttpStatusCode.ServiceUnavailable,
                "Service is unavailable. Please try again later.", new Dictionary<string, string>(), null);
        }
        return await ToResultAsync(response, ct);
    }

    public async Task<AuthApiResult> SetDefaultAddressAsync(int id, CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try { response = await _http.PostAsync($"api/addresses/{id}/default", null, ct); }
        catch (HttpRequestException)
        {
            return new AuthApiResult(false, HttpStatusCode.ServiceUnavailable,
                "Service is unavailable. Please try again later.", new Dictionary<string, string>(), null);
        }
        return await ToResultAsync(response, ct);
    }

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

        return await ToResultAsync(response, ct);
    }

    /// <summary>Normalizes an API response into an <see cref="AuthApiResult"/>.</summary>
    private static async Task<AuthApiResult> ToResultAsync(HttpResponseMessage response, CancellationToken ct)
    {
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
