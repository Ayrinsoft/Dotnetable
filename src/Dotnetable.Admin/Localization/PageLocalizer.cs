using Dotnetable.Admin.Auth;
using Dotnetable.Application;
using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Data;
using Microsoft.AspNetCore.Components.Authorization;

namespace Dotnetable.Admin.Localization;

/// <inheritdoc cref="IPageLocalizer"/>
public sealed class PageLocalizer : IPageLocalizer
{
    // Admin UI strings default to English; translations for other languages come from the DB.
    public const string DefaultLanguage = "en";

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

    public int WebsiteId { get; private set; } = AppConstants.MasterWebsiteId;
    public string LanguageCode { get; private set; } = DefaultLanguage;

    // This instance is scoped per circuit, but several components can call EnsureLoadedAsync
    // concurrently from their own OnInitializedAsync. Without coordination, two callers would both
    // see _loaded == false and both drive the same scoped AppDbContext at once, throwing
    // "A second operation was started on this context instance before a previous operation
    // completed." Cache the in-flight task so concurrent callers await the same load.
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

        await _localization.LoadAsync(WebsiteId, LanguageCode, ct);
        _loaded = true;
    }

    // "dn-lang" cookie (set by the header switcher / auth pages) wins; otherwise fall back to the
    // member's website default language.
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
            if (_cache.TryGet(WebsiteId, LanguageCode, key, out var value))
                return value;

            // Before the website/language is resolved, just return the default without side effects —
            // registering now could file the key under the wrong website.
            if (!_loaded) return defaultValue;

            // Unknown key: queue it for insertion and cache the default so we neither re-queue it
            // nor block rendering on a DB round-trip.
            _pending.Add(WebsiteId, key, defaultValue);
            _cache.Set(WebsiteId, LanguageCode, key, defaultValue);
            return defaultValue;
        }
    }
}
