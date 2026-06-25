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
    private bool _loaded;

    public PageLocalizer(
        TranslationCache cache,
        ILocalizationService localization,
        PendingTranslationKeys pending,
        AuthenticationStateProvider authState)
    {
        _cache = cache;
        _localization = localization;
        _pending = pending;
        _authState = authState;
    }

    public int WebsiteId { get; private set; } = AppConstants.MasterWebsiteId;
    public string LanguageCode { get; private set; } = DefaultLanguage;

    public async Task EnsureLoadedAsync(CancellationToken ct = default)
    {
        if (_loaded) return;

        var state = await _authState.GetAuthenticationStateAsync();
        if (int.TryParse(state.User.FindFirst(AdminClaimTypes.WebsiteId)?.Value, out var websiteId) && websiteId > 0)
            WebsiteId = websiteId;

        await _localization.LoadAsync(WebsiteId, LanguageCode, ct);
        _loaded = true;
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
