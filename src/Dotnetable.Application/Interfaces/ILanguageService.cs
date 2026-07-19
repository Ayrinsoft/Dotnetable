using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Two independent uses of the <see cref="Language"/> table (same schema, different rows):
/// <list type="bullet">
///   <item><b>Admin catalog</b> — rows with <c>WebsiteID = null</c>. Drive the admin UI language
///   switcher and Initial Data → Languages. Unrelated to any website's storefront.</item>
///   <item><b>Per-website languages</b> — rows with <c>WebsiteID</c> set (including master site 1).
///   Drive storefront language lists, content translation tabs, and Website → Languages.
///   A new site starts with only its default language; more languages are added later when needed.</item>
/// </list>
/// </summary>
public interface ILanguageService
{
    // ── Admin catalog (WebsiteID null) ───────────────────────────────────────

    /// <summary>Admin catalog, ordered by Priority. Self-seeds built-in defaults when empty.</summary>
    Task<List<Language>> GetCatalogAsync(CancellationToken ct = default);

    /// <summary>Admin catalog, paged/sorted/searched for the admin grid.</summary>
    Task<PagedResult<Language>> GetCatalogPagedAsync(GridQuery query, CancellationToken ct = default);

    /// <summary>Active subset of the admin catalog (admin language switcher).</summary>
    Task<List<Language>> GetActiveCatalogAsync(CancellationToken ct = default);

    /// <summary>
    /// Active non-default languages from the admin catalog. Used when admin-scoped content
    /// (e.g. Initial Data translations) needs "other language" fields. Empty when only one
    /// active language exists — the main form fields already cover the default.
    /// </summary>
    Task<List<Language>> GetOtherActiveCatalogAsync(CancellationToken ct = default);

    /// <summary>Adds a language to the admin catalog (<c>WebsiteID = null</c>).</summary>
    Task<Language> CreateAsync(Language language, CancellationToken ct = default);

    /// <summary>Updates an admin-catalog language.</summary>
    Task<bool> UpdateAsync(Language language, CancellationToken ct = default);

    /// <summary>Soft-hides/restores an admin-catalog language.</summary>
    Task<bool> SetActiveAsync(int languageId, bool active, CancellationToken ct = default);

    // ── Per-website languages (storefront / content) ─────────────────────────

    /// <summary>All language rows for a website (active and inactive), ordered by Priority.
    /// Ensures the site default language exists (only that language when the site has none yet).</summary>
    Task<List<Language>> GetForWebsiteAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Active languages for a website. Ensures the site default language exists when empty.</summary>
    Task<List<Language>> GetActiveForWebsiteAsync(int websiteId, CancellationToken ct = default);

    /// <summary>
    /// Active non-default languages for a website (including master site 1). Content editors use this
    /// for "other languages" translation panels: main fields are the site default; leave a translation
    /// blank to fall back to the default-language value at display time. Empty when the site has only
    /// one active language (or only the default). Inactive languages are never returned.
    /// </summary>
    Task<List<Language>> GetOtherActiveForWebsiteAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Adds a language owned by this website only (not the admin catalog).</summary>
    Task<Language> AddWebsiteLanguageAsync(int websiteId, Language language, CancellationToken ct = default);

    /// <summary>Updates a language row that belongs to <paramref name="websiteId"/>.</summary>
    Task<bool> UpdateWebsiteLanguageAsync(int websiteId, Language language, CancellationToken ct = default);

    /// <summary>Activates or deactivates a website language. The default language cannot be deactivated.</summary>
    Task<bool> SetWebsiteLanguageActiveAsync(int websiteId, int languageId, bool active, CancellationToken ct = default);

    /// <summary>Marks one website language as default (and active). Clears default on the others.</summary>
    Task<bool> SetWebsiteDefaultLanguageAsync(int websiteId, int languageId, CancellationToken ct = default);

    /// <summary>Removes a website language. Refuses to remove the only / default language.</summary>
    Task<bool> RemoveWebsiteLanguageAsync(int websiteId, string languageCode, CancellationToken ct = default);
}
