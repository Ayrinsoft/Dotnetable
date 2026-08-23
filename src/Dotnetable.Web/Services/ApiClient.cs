using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Dotnetable.Web.Services;

/// <summary>A shipping method resolved with its computed price for the current cart/address.</summary>
public sealed class ShippingOptionDto
{
    public int ShippingMethodID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CarrierName { get; set; }
    public MoneyDto? Price { get; set; }
    /// <summary>Bridge/legacy dual amount (same as <see cref="Price"/>.AmountUsd when present).</summary>
    public decimal PriceUsd { get; set; }
}

/// <summary>The website's own bank account, for manual/offline customer transfers.</summary>
public sealed class OfflineBankAccountDto
{
    public int BankAccountID { get; set; }
    public string? BankName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public string? IBAN { get; set; }
    public string? CardNumber { get; set; }
}

/// <summary>A flattened wishlist row as returned by the API.</summary>
public sealed class WishlistItemView
{
    public int WishlistItemID { get; set; }
    public int ProductVariantID { get; set; }
    public int ProductID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public MoneyDto? Price { get; set; }
    public decimal PriceUsd { get; set; }
}

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

    /// <summary>GET that yields null for "no content" responses instead of throwing: the API answers
    /// 204 (nothing assigned/found) and 404 (unknown slug / unresolved website) on public content
    /// reads, and <c>GetFromJsonAsync</c> would throw a <see cref="JsonException"/> on the empty
    /// body. Unreachable-service and malformed-body errors also fold into null so pages degrade
    /// gracefully instead of surfacing a 500.</summary>
    private async Task<T?> GetOrNullAsync<T>(string path, CancellationToken ct) where T : class
    {
        try
        {
            using var response = await _http.GetAsync(path, ct);
            if (response.StatusCode == HttpStatusCode.NoContent || !response.IsSuccessStatusCode)
                return null;
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
        }
        catch (HttpRequestException) { return null; }
        catch (NotSupportedException) { return null; }
        catch (JsonException) { return null; }
    }

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

            // 204 No Content (no menu assigned) folds into null, which is exactly what we want.
            return await GetOrNullAsync<MenuDto>(path, ct);
        });

    /// <summary>Fetches the active slideshow assigned to a placement key (e.g. "home_top"), or null
    /// when none is configured / the API is unreachable — callers simply render nothing.</summary>
    public Task<SlideshowDto?> GetSlideshowByPlacementAsync(string placementKey, CancellationToken ct = default) =>
        CachedGetAsync($"slideshow:placement:{placementKey}", async () =>
        {
            var path = $"api/slideshow/placement/{Uri.EscapeDataString(placementKey)}";
            return await GetOrNullAsync<SlideshowDto>(path, ct);
        });

    /// <summary>Fetches a single active slideshow by id — used to resolve a <c>[slideshow:ID]</c>
    /// shortcode embedded inside a Post/Page body.</summary>
    public Task<SlideshowDto?> GetSlideshowByIdAsync(int slideshowId, CancellationToken ct = default) =>
        CachedGetAsync($"slideshow:id:{slideshowId}", async () =>
        {
            var path = $"api/slideshow/{slideshowId}";
            return await GetOrNullAsync<SlideshowDto>(path, ct);
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
        return await GetOrNullAsync<PostDetailDto>(path, ct);
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
            return await GetOrNullAsync<PageDto>(path, ct);
        });

    /// <summary>Submits a visitor-filled contact form to the API's contact inbox.</summary>
    public Task<AuthApiResult> SubmitContactMessageAsync(ContactMessageRequest request, CancellationToken ct = default) =>
        PostAsync("api/contact", request, ct);

    /// <summary>Fetches a fresh captcha challenge (Turnstile site key or a math-captcha SVG) for
    /// this website's public forms, or null when the API is unreachable.</summary>
    public Task<CaptchaChallengeDto?> GetCaptchaChallengeAsync(CancellationToken ct = default) =>
        GetOrNullAsync<CaptchaChallengeDto>("api/captcha/challenge", ct);

    /// <summary>Site branding/identity (brand, logo, contact, socials, SEO defaults) used by the
    /// layout. Null when the API is unreachable — the layout falls back to neutral defaults.</summary>
    public Task<SiteInfoDto?> GetSiteInfoAsync(CancellationToken ct = default) =>
        CachedGetAsync("siteinfo", () => GetOrNullAsync<SiteInfoDto>("api/siteinfo", ct));

    /// <summary>Active WordPress-style theme package (view root under Themes/). Null when the API
    /// is unreachable — Web falls back to Theme:Active / Default.</summary>
    public Task<ActiveThemeDto?> GetActiveThemeAsync(CancellationToken ct = default) =>
        CachedGetAsync("active-theme", () => GetOrNullAsync<ActiveThemeDto>("api/theme/active", ct));

    /// <summary>This website's own active languages (Languages rows for the site, managed under
    /// Website → Languages), for the front-end language switcher. Empty list when the API is unreachable.</summary>
    public async Task<IReadOnlyList<LanguageDto>> GetActiveLanguagesAsync(CancellationToken ct = default) =>
        await CachedGetAsync<IReadOnlyList<LanguageDto>>("languages", () => GetOrNullAsync<IReadOnlyList<LanguageDto>>("api/languages", ct))
        ?? Array.Empty<LanguageDto>();

    // ── Dynamic forms & surveys ─────────────────────────────────────

    /// <summary>An active dynamic form/survey by public slug, or null.</summary>
    public Task<FormDto?> GetFormBySlugAsync(string slug, CancellationToken ct = default) =>
        CachedGetAsync($"form:slug:{slug}", () => GetOrNullAsync<FormDto>($"api/forms/{Uri.EscapeDataString(slug)}", ct));

    /// <summary>An active dynamic form/survey by id — resolves a <c>[form:ID]</c> shortcode.</summary>
    public Task<FormDto?> GetFormByIdAsync(int formId, CancellationToken ct = default) =>
        CachedGetAsync($"form:id:{formId}", () => GetOrNullAsync<FormDto>($"api/forms/id/{formId}", ct));

    /// <summary>Submits a visitor's answers to a dynamic form. Validation happens server-side; a
    /// failed submission returns Ok=false with a user-displayable message.</summary>
    public async Task<FormSubmissionResult> SubmitFormAsync(int formId, FormSubmissionRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"api/forms/{formId}/submit", request, ct);
            var result = await response.Content.ReadFromJsonAsync<FormSubmissionResult>(cancellationToken: ct);
            return result ?? (response.IsSuccessStatusCode
                ? FormSubmissionResult.Success()
                : FormSubmissionResult.Fail("Could not submit the form. Please try again."));
        }
        catch (HttpRequestException) { return FormSubmissionResult.Fail("Could not submit the form. Please try again."); }
        catch (NotSupportedException) { return FormSubmissionResult.Fail("Could not submit the form. Please try again."); }
    }

    /// <summary>Public aggregate survey results (only for forms with public results enabled), or null.</summary>
    public async Task<FormPublicResultsDto?> GetFormResultsAsync(int formId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"api/forms/{formId}/results", ct);
            if (response.StatusCode == HttpStatusCode.NoContent || !response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<FormPublicResultsDto>(cancellationToken: ct);
        }
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

    // ── Catalog ──────────────────────────────────────────────────────

    public async Task<PagedResult<ProductSummaryDto>> GetProductsAsync(
        string? category = null, string? brand = null, string? search = null,
        decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 20,
        string? lang = null, string? currency = null, bool? inStock = null,
        IReadOnlyList<int>? attributeOptionIds = null, CancellationToken ct = default)
    {
        var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(category)) query.Add($"categorySlug={Uri.EscapeDataString(category)}");
        if (!string.IsNullOrWhiteSpace(brand)) query.Add($"brandSlug={Uri.EscapeDataString(brand)}");
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        if (minPrice is decimal min) query.Add($"minPrice={min}");
        if (maxPrice is decimal max) query.Add($"maxPrice={max}");
        if (!string.IsNullOrWhiteSpace(lang)) query.Add($"lang={Uri.EscapeDataString(lang)}");
        if (!string.IsNullOrWhiteSpace(currency)) query.Add($"currency={Uri.EscapeDataString(currency)}");
        if (inStock is bool stockFilter) query.Add($"inStock={(stockFilter ? "true" : "false")}");
        if (attributeOptionIds is { Count: > 0 })
        {
            foreach (var id in attributeOptionIds.Where(x => x > 0).Distinct())
                query.Add($"attributeOptionIds={id}");
        }

        try
        {
            return await _http.GetFromJsonAsync<PagedResult<ProductSummaryDto>>($"api/products?{string.Join('&', query)}", ct)
                ?? new PagedResult<ProductSummaryDto>();
        }
        catch (HttpRequestException) { return new PagedResult<ProductSummaryDto>(); }
    }

    public async Task<ProductDetailDto?> GetProductAsync(string slug, string? lang = null, string? currency = null, CancellationToken ct = default)
    {
        var path = $"api/products/{Uri.EscapeDataString(slug)}";
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(lang)) query.Add($"lang={Uri.EscapeDataString(lang)}");
        if (!string.IsNullOrWhiteSpace(currency)) query.Add($"currency={Uri.EscapeDataString(currency)}");
        if (query.Count > 0) path += $"?{string.Join('&', query)}";
        return await GetOrNullAsync<ProductDetailDto>(path, ct);
    }

    public async Task<IReadOnlyList<ProductCategoryDto>> GetProductCategoryTreeAsync(string? lang = null, CancellationToken ct = default)
    {
        var path = "api/productcategories/tree" + (string.IsNullOrWhiteSpace(lang) ? "" : $"?lang={Uri.EscapeDataString(lang)}");
        try { return await _http.GetFromJsonAsync<List<ProductCategoryDto>>(path, ct) ?? new(); }
        catch (HttpRequestException) { return Array.Empty<ProductCategoryDto>(); }
    }

    /// <summary>Filterable attribute facets for a product category (including ancestors).</summary>
    public async Task<IReadOnlyList<CategoryAttributeFilterDto>> GetProductCategoryFiltersAsync(
        string categorySlug, string? lang = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(categorySlug)) return Array.Empty<CategoryAttributeFilterDto>();
        var path = $"api/productcategories/{Uri.EscapeDataString(categorySlug)}/filters";
        if (!string.IsNullOrWhiteSpace(lang)) path += $"?lang={Uri.EscapeDataString(lang)}";
        try { return await _http.GetFromJsonAsync<List<CategoryAttributeFilterDto>>(path, ct) ?? new(); }
        catch (HttpRequestException) { return Array.Empty<CategoryAttributeFilterDto>(); }
    }

    public async Task<IReadOnlyList<BrandDto>> GetBrandsAsync(string? lang = null, CancellationToken ct = default)
    {
        var path = "api/brands" + (string.IsNullOrWhiteSpace(lang) ? "" : $"?lang={Uri.EscapeDataString(lang)}");
        try { return await _http.GetFromJsonAsync<List<BrandDto>>(path, ct) ?? new(); }
        catch (HttpRequestException) { return Array.Empty<BrandDto>(); }
    }

    // ── Cart ─────────────────────────────────────────────────────────

    public async Task<CartViewDto?> GetCartAsync(string? currency = null, CancellationToken ct = default)
    {
        var path = "api/cart" + (string.IsNullOrWhiteSpace(currency) ? "" : $"?currency={Uri.EscapeDataString(currency)}");
        return await GetOrNullAsync<CartViewDto>(path, ct);
    }

    public Task<AuthApiResult> AddToCartAsync(int variantId, int quantity, int? vendorProductId = null, int? vendorId = null, CancellationToken ct = default) =>
        PostAsync("api/cart/items", new { variantId, quantity, vendorProductId, vendorId }, ct);

    public async Task<AuthApiResult> UpdateCartItemAsync(int cartItemId, int quantity, CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try { response = await _http.PutAsJsonAsync($"api/cart/items/{cartItemId}", new { quantity }, ct); }
        catch (HttpRequestException) { return Unreachable(); }
        return await ToResultAsync(response, ct);
    }

    public async Task<AuthApiResult> RemoveCartItemAsync(int cartItemId, CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try { response = await _http.DeleteAsync($"api/cart/items/{cartItemId}", ct); }
        catch (HttpRequestException) { return Unreachable(); }
        return await ToResultAsync(response, ct);
    }

    public Task<AuthApiResult> ApplyCouponAsync(string code, CancellationToken ct = default) =>
        PostAsync("api/cart/coupon", new { code }, ct);

    public async Task<AuthApiResult> RemoveCouponAsync(CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try { response = await _http.DeleteAsync("api/cart/coupon", ct); }
        catch (HttpRequestException) { return Unreachable(); }
        return await ToResultAsync(response, ct);
    }

    /// <summary>
    /// Folds the guest cart into the signed-in customer's cart — call right after login. Takes the
    /// freshly-issued token explicitly (rather than relying on <see cref="BearerTokenHandler"/>'s
    /// cookie read) because the session cookie set on the Response isn't visible on the current
    /// Request yet.
    /// </summary>
    public async Task<AuthApiResult> MergeCartAsync(string accessToken, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/cart/merge");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage response;
        try { response = await _http.SendAsync(request, ct); }
        catch (HttpRequestException) { return Unreachable(); }
        return await ToResultAsync(response, ct);
    }

    // ── Shipping ─────────────────────────────────────────────────────

    public async Task<IReadOnlyList<ShippingOptionDto>> GetShippingOptionsAsync(
        int? countryId, int? stateId, int? cityId, decimal weightKg, decimal cartSubtotal = 0, CancellationToken ct = default)
    {
        var query = new List<string> { $"weightKg={weightKg}", $"cartSubtotal={cartSubtotal}" };
        if (countryId is int c) query.Add($"countryId={c}");
        if (stateId is int s) query.Add($"stateId={s}");
        if (cityId is int ci) query.Add($"cityId={ci}");
        try { return await _http.GetFromJsonAsync<List<ShippingOptionDto>>($"api/shipping/methods?{string.Join('&', query)}", ct) ?? new(); }
        catch (HttpRequestException) { return Array.Empty<ShippingOptionDto>(); }
    }

    // ── Checkout / Orders ────────────────────────────────────────────

    public async Task<(bool Success, string? Error, int? OrderId, string? OrderNumber)> CheckoutAsync(
        int cartId, int addressId, int shippingMethodId, string? currency, CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsJsonAsync("api/checkout", new { cartId, addressId, shippingMethodId, currency }, ct);
        }
        catch (HttpRequestException) { return (false, "Service is unavailable.", null, null); }

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(cancellationToken: ct);
            return (false, err?.GetValueOrDefault("message"), null, null);
        }

        var ok = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>(cancellationToken: ct);
        return (true, null, ok?["orderId"].GetInt32(), ok?["orderNumber"].GetString());
    }

    public async Task<PagedResult<Order>> GetOrdersAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<PagedResult<Order>>($"api/orders?page={page}&pageSize={pageSize}", ct)
                ?? new PagedResult<Order>();
        }
        catch (HttpRequestException) { return new PagedResult<Order>(); }
    }

    public async Task<Order?> GetOrderAsync(int id, CancellationToken ct = default)
    {
        return await GetOrNullAsync<Order>($"api/orders/{id}", ct);
    }

    public async Task<PagedResult<SupportSessionSummaryDto>> GetSupportTicketsAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<PagedResult<SupportSessionSummaryDto>>($"api/support?page={page}&pageSize={pageSize}", ct)
                ?? new PagedResult<SupportSessionSummaryDto>();
        }
        catch (HttpRequestException) { return new PagedResult<SupportSessionSummaryDto>(); }
    }

    public Task<ClientSupportTicketDetailDto?> GetSupportTicketAsync(int id, CancellationToken ct = default) =>
        GetOrNullAsync<ClientSupportTicketDetailDto>($"api/support/{id}", ct);

    public async Task<(bool Ok, SupportSessionSummaryDto? Row, string? Error)> CreateSupportTicketAsync(object payload, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/support", payload, ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(cancellationToken: ct);
                return (false, null, err?.GetValueOrDefault("message") ?? "Request failed.");
            }
            var row = await response.Content.ReadFromJsonAsync<SupportSessionSummaryDto>(cancellationToken: ct);
            return (true, row, null);
        }
        catch (HttpRequestException) { return (false, null, "Service is unavailable."); }
    }

    public async Task<(bool Ok, string? Error)> ReplySupportTicketAsync(int id, string body, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"api/support/{id}/replies", new { body }, ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(cancellationToken: ct);
                return (false, err?.GetValueOrDefault("message") ?? "Request failed.");
            }
            return (true, null);
        }
        catch (HttpRequestException) { return (false, "Service is unavailable."); }
    }

    public async Task<PagedResult<CustomerReturnDto>> GetReturnsAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<PagedResult<CustomerReturnDto>>($"api/returns?page={page}&pageSize={pageSize}", ct)
                ?? new PagedResult<CustomerReturnDto>();
        }
        catch (HttpRequestException) { return new PagedResult<CustomerReturnDto>(); }
    }

    public Task<CustomerReturnDto?> GetReturnAsync(int id, CancellationToken ct = default) =>
        GetOrNullAsync<CustomerReturnDto>($"api/returns/{id}", ct);

    public Task<ReturnEligibilityDto?> GetReturnEligibilityAsync(int orderId, CancellationToken ct = default) =>
        GetOrNullAsync<ReturnEligibilityDto>($"api/returns/eligible/{orderId}", ct);

    public async Task<(bool Ok, CustomerReturnDto? Row, string? Error)> CreateReturnAsync(object payload, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/returns", payload, ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(cancellationToken: ct);
                return (false, null, err?.GetValueOrDefault("message") ?? "Request failed.");
            }
            var row = await response.Content.ReadFromJsonAsync<CustomerReturnDto>(cancellationToken: ct);
            return (true, row, null);
        }
        catch (HttpRequestException) { return (false, null, "Service is unavailable."); }
    }

    public Task<AuthApiResult> ShipReturnAsync(int id, string trackingCode, string? shipMethod, CancellationToken ct = default) =>
        PostAsync($"api/returns/{id}/ship", new { trackingCode, shipMethod }, ct);

    public async Task<AuthApiResult> UpdateReturnTrackingAsync(int id, string trackingCode, string? shipMethod, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/returns/{id}/tracking", new { trackingCode, shipMethod }, ct);
            return await ToResultAsync(response, ct);
        }
        catch (HttpRequestException) { return Unreachable(); }
    }

    public async Task<(bool Ok, int? FileId, string? Error)> UploadReturnPhotoAsync(int returnId, IFormFile file, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        using var streamContent = new StreamContent(stream);
        if (!string.IsNullOrEmpty(file.ContentType))
            streamContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(file.ContentType);
        content.Add(streamContent, "file", file.FileName);
        try
        {
            var response = await _http.PostAsync($"api/returns/{returnId}/photos", content, ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(cancellationToken: ct);
                return (false, null, err?.GetValueOrDefault("message"));
            }
            var ok = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>(cancellationToken: ct);
            return (true, ok?.GetValueOrDefault("fileId"), null);
        }
        catch (HttpRequestException) { return (false, null, "Service is unavailable."); }
    }

    // ── Digital library (post-purchase downloads / codes / service URLs) ──

    public async Task<PagedResult<DigitalLibraryItemDto>> GetDigitalLibraryAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<PagedResult<DigitalLibraryItemDto>>(
                       $"api/digital?page={page}&pageSize={pageSize}", ct)
                   ?? new PagedResult<DigitalLibraryItemDto>();
        }
        catch (HttpRequestException) { return new PagedResult<DigitalLibraryItemDto>(); }
    }

    public async Task<IReadOnlyList<DigitalLibraryItemDto>> GetOrderDigitalAsync(int orderId, CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<DigitalLibraryItemDto>>($"api/digital/order/{orderId}", ct)
                   ?? new List<DigitalLibraryItemDto>();
        }
        catch (HttpRequestException) { return Array.Empty<DigitalLibraryItemDto>(); }
    }

    public Task<DigitalLibraryDetailDto?> GetDigitalDetailAsync(int id, CancellationToken ct = default) =>
        GetOrNullAsync<DigitalLibraryDetailDto>($"api/digital/{id}", ct);

    public Task<AuthApiResult> LogDigitalAccessAsync(int id, string accessType, CancellationToken ct = default) =>
        PostAsync($"api/digital/{id}/access", new { accessType }, ct);

    /// <summary>
    /// Calls the API download endpoint (logs access) and returns the external download URL.
    /// </summary>
    public async Task<string?> ResolveDigitalDownloadUrlAsync(int id, CancellationToken ct = default)
    {
        try
        {
            using var response = await _http.GetAsync($"api/digital/{id}/download", ct);
            if (!response.IsSuccessStatusCode) return null;
            var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>(cancellationToken: ct);
            if (payload is not null && payload.TryGetValue("downloadUrl", out var urlEl) && urlEl.ValueKind == JsonValueKind.String)
                return urlEl.GetString();
            return null;
        }
        catch (HttpRequestException) { return null; }
        catch (JsonException) { return null; }
    }

    // ── Payments ─────────────────────────────────────────────────────

    public Task<AuthApiResult> PayWithWalletAsync(int orderId, CancellationToken ct = default) =>
        PostAsync("api/payments/wallet", new { orderId }, ct);

    public Task<AuthApiResult> SubmitBankReceiptAsync(int orderId, int bankAccountId, int receiptFileId, CancellationToken ct = default) =>
        PostAsync("api/payments/receipt", new { orderId, bankAccountId, receiptFileId }, ct);

    public async Task<IReadOnlyList<OfflineBankAccountDto>> GetOfflineBankAccountsAsync(CancellationToken ct = default)
    {
        try { return await _http.GetFromJsonAsync<List<OfflineBankAccountDto>>("api/payments/bank-accounts", ct) ?? new(); }
        catch (HttpRequestException) { return Array.Empty<OfflineBankAccountDto>(); }
    }

    public async Task<(bool Ok, int? FileId, string? Error)> UploadReceiptAsync(IFormFile file, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        using var streamContent = new StreamContent(stream);
        if (!string.IsNullOrEmpty(file.ContentType))
            streamContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(file.ContentType);
        content.Add(streamContent, "file", file.FileName);

        HttpResponseMessage response;
        try { response = await _http.PostAsync("api/payments/receipt-upload", content, ct); }
        catch (HttpRequestException) { return (false, null, "Service is unavailable."); }

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(cancellationToken: ct);
            return (false, null, err?.GetValueOrDefault("message"));
        }

        var ok = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>(cancellationToken: ct);
        return (true, ok?.GetValueOrDefault("fileId"), null);
    }

    // ── Wallet ───────────────────────────────────────────────────────

    public async Task<WalletBalanceDto> GetWalletBalanceAsync(CancellationToken ct = default)
    {
        try { return await _http.GetFromJsonAsync<WalletBalanceDto>("api/wallet", ct) ?? new(); }
        catch (HttpRequestException) { return new(); }
    }

    public async Task<PagedResult<WalletTransactionDto>> GetWalletTransactionsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<PagedResult<WalletTransactionDto>>($"api/wallet/transactions?pageIndex={page}&pageSize={pageSize}", ct)
                ?? new PagedResult<WalletTransactionDto>();
        }
        catch (HttpRequestException) { return new PagedResult<WalletTransactionDto>(); }
    }

    public Task<AuthApiResult> RequestWithdrawalAsync(int clientBankAccountId, decimal amountUsd, CancellationToken ct = default) =>
        PostAsync("api/wallet/withdrawals", new { clientBankAccountId, amountUsd }, ct);

    public async Task<IReadOnlyList<ClientBankAccountDto>> GetClientBankAccountsAsync(CancellationToken ct = default)
    {
        try { return await _http.GetFromJsonAsync<List<ClientBankAccountDto>>("api/clientbankaccounts", ct) ?? new(); }
        catch (HttpRequestException) { return Array.Empty<ClientBankAccountDto>(); }
    }

    public Task<AuthApiResult> CreateClientBankAccountAsync(ClientBankAccountRequest request, CancellationToken ct = default) =>
        PostAsync("api/clientbankaccounts", request, ct);

    // ── Wishlist ─────────────────────────────────────────────────────

    public async Task<IReadOnlyList<WishlistItemView>> GetWishlistAsync(CancellationToken ct = default)
    {
        try { return await _http.GetFromJsonAsync<List<WishlistItemView>>("api/wishlist", ct) ?? new(); }
        catch (HttpRequestException) { return Array.Empty<WishlistItemView>(); }
    }

    public Task<AuthApiResult> AddToWishlistAsync(int variantId, CancellationToken ct = default) =>
        PostAsync("api/wishlist/items", new { variantId }, ct);

    public async Task<AuthApiResult> RemoveFromWishlistAsync(int variantId, CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try { response = await _http.DeleteAsync($"api/wishlist/items/{variantId}", ct); }
        catch (HttpRequestException) { return Unreachable(); }
        return await ToResultAsync(response, ct);
    }

    // ── Reviews & Q&A ────────────────────────────────────────────────

    public async Task<PagedResult<ProductReview>> GetProductReviewsAsync(int productId, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<PagedResult<ProductReview>>($"api/products/{productId}/reviews?page={page}&pageSize={pageSize}", ct)
                ?? new PagedResult<ProductReview>();
        }
        catch (HttpRequestException) { return new PagedResult<ProductReview>(); }
    }

    public Task<AuthApiResult> SubmitReviewAsync(int productId, byte rating, string? title, string body, CancellationToken ct = default) =>
        PostAsync($"api/products/{productId}/reviews", new { rating, title, body }, ct);

    public async Task<PagedResult<ProductQuestion>> GetProductQuestionsAsync(int productId, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<PagedResult<ProductQuestion>>($"api/products/{productId}/questions?page={page}&pageSize={pageSize}", ct)
                ?? new PagedResult<ProductQuestion>();
        }
        catch (HttpRequestException) { return new PagedResult<ProductQuestion>(); }
    }

    public Task<AuthApiResult> AskQuestionAsync(int productId, string body, CancellationToken ct = default) =>
        PostAsync($"api/products/{productId}/questions", new { body }, ct);

    public Task<AuthApiResult> AnswerQuestionAsync(int questionId, string body, CancellationToken ct = default) =>
        PostAsync($"api/questions/{questionId}/answers", new { body }, ct);

    private static AuthApiResult Unreachable() => new(false, HttpStatusCode.ServiceUnavailable,
        "Service is unavailable. Please try again later.", new Dictionary<string, string>(), null);

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
