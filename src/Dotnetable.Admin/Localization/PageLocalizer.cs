using Dotnetable.Admin.Auth;
using Dotnetable.Application;
using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Data;
using Microsoft.AspNetCore.Components.Authorization;

namespace Dotnetable.Admin.Localization;

/// <inheritdoc cref="IPageLocalizer"/>
public sealed class PageLocalizer : IPageLocalizer
{
    // Admin UI strings default to English; translations live under LocalizationKeys with WebsiteID = null.
    public const string DefaultLanguage = "en";

    /// <summary>Cache / pending key for admin catalog (maps to DB WebsiteID = null).</summary>
    private const int AdminCacheWebsiteId = 0;

    private readonly TranslationCache _cache;
    private readonly ILocalizationService _localization;
    private readonly PendingTranslationKeys _pending;
    private readonly AuthenticationStateProvider _authState;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebsiteService _websiteService;
    private readonly ILanguageService _languageService;
    private readonly object _loadLock = new();
    private volatile bool _loaded;
    private Task? _loadTask;

    public PageLocalizer(
        TranslationCache cache,
        ILocalizationService localization,
        PendingTranslationKeys pending,
        AuthenticationStateProvider authState,
        IHttpContextAccessor httpContextAccessor,
        IWebsiteService websiteService,
        ILanguageService languageService)
    {
        _cache = cache;
        _localization = localization;
        _pending = pending;
        _authState = authState;
        _httpContextAccessor = httpContextAccessor;
        _websiteService = websiteService;
        _languageService = languageService;
    }

    /// <summary>Signed-in member's website (for content language pickers, etc.). Admin UI strings
    /// always load from the admin catalog (<c>WebsiteID = null</c>), never from a site bag.</summary>
    public int WebsiteId { get; private set; } = AppConstants.MasterWebsiteId;
    public string LanguageCode { get; private set; } = DefaultLanguage;

    public Task EnsureLoadedAsync(CancellationToken ct = default)
    {
        if (_loaded) return Task.CompletedTask;

        lock (_loadLock)
        {
            if (_loaded) return Task.CompletedTask;
            _loadTask ??= LoadCoreAsync(ct);
            return _loadTask;
        }
    }

    private async Task LoadCoreAsync(CancellationToken ct)
    {
        var state = await _authState.GetAuthenticationStateAsync();
        if (int.TryParse(state.User.FindFirst(AdminClaimTypes.WebsiteId)?.Value, out var websiteId) && websiteId > 0)
            WebsiteId = websiteId;

        LanguageCode = await ResolveLanguageAsync(ct);

        // Admin panel vocabulary: LocalizationKeys.WebsiteID IS NULL — not master site 1.
        await _localization.LoadAsync(null, LanguageCode, ct);
        _loaded = true;
    }

    private async Task<string> ResolveLanguageAsync(CancellationToken ct)
    {
        var options = (await _languageService.GetActiveCatalogAsync(ct)).Select(l => l.ToOption()).ToList();

        var cookie = _httpContextAccessor.HttpContext?.Request.Cookies[SupportedLanguages.CookieName];
        if (options.IsSupported(cookie))
            return cookie!;

        var website = await _websiteService.GetByIdAsync(WebsiteId, ct);
        return options.IsSupported(website?.DefaultLanguageCode)
            ? website!.DefaultLanguageCode
            : DefaultLanguage;
    }

    public string this[string key] => this[key, key];

    public string this[string key, string defaultValue]
    {
        get
        {
            if (_cache.TryGet(AdminCacheWebsiteId, LanguageCode, key, out var value))
                return value;

            if (!_loaded) return defaultValue;

            // Queue under admin catalog (WebsiteID null in DB).
            _pending.Add(null, key, defaultValue);
            _cache.Set(AdminCacheWebsiteId, LanguageCode, key, defaultValue);
            return defaultValue;
        }
    }
}
