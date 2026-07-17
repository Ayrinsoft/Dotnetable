using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// The master website's <see cref="Language"/> rows double as the system-wide language catalog
/// (managed only from the master website); every other website has its own subset of rows in the
/// same table marking which catalog languages it offers for its content.
/// </summary>
public interface ILanguageService
{
    /// <summary>The master catalog, ordered by Priority. Self-seeds the built-in defaults on first
    /// call if the table is still empty (e.g. an existing install predating this feature).</summary>
    Task<List<Language>> GetCatalogAsync(CancellationToken ct = default);

    /// <summary>The master catalog, paged/sorted/searched per the admin grid's state (same
    /// Code/ISO/Name search + sort convention as every other list page in the panel).</summary>
    Task<PagedResult<Language>> GetCatalogPagedAsync(GridQuery query, CancellationToken ct = default);

    /// <summary>The active subset of the master catalog.</summary>
    Task<List<Language>> GetActiveCatalogAsync(CancellationToken ct = default);

    /// <summary>Adds a new language to the master catalog.</summary>
    Task<Language> CreateAsync(Language language, CancellationToken ct = default);

    /// <summary>Updates a master-catalog language's editable fields.</summary>
    Task<bool> UpdateAsync(Language language, CancellationToken ct = default);

    /// <summary>Soft removes/restores a master-catalog language — hides it from every pick-list
    /// without touching translation data that already used it.</summary>
    Task<bool> SetActiveAsync(int languageId, bool active, CancellationToken ct = default);

    /// <summary>The given website's own active languages. For the master website this is the same
    /// as <see cref="GetActiveCatalogAsync"/>. Falls back to the website's default language (or
    /// "en") if the website hasn't picked any yet.</summary>
    Task<List<Language>> GetActiveForWebsiteAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Sets a non-master website's own enabled languages to exactly the given codes (must
    /// be a subset of the active master catalog — anything else is silently ignored).</summary>
    Task SetWebsiteLanguagesAsync(int websiteId, IEnumerable<string> codes, CancellationToken ct = default);

    /// <summary>Adds a language of the website's own choosing — not restricted to the master catalog
    /// — visible only to that website. Throws if the website already has that code. No-op for the
    /// master website (use <see cref="CreateAsync"/> to add to the shared catalog instead).</summary>
    Task<Language> AddWebsiteLanguageAsync(int websiteId, Language language, CancellationToken ct = default);

    /// <summary>Removes a language a website added for itself via <see cref="AddWebsiteLanguageAsync"/>.
    /// No-op for the master website — catalog rows are hidden with <see cref="SetActiveAsync"/> instead
    /// of being deleted, since translation data may already reference them.</summary>
    Task<bool> RemoveWebsiteLanguageAsync(int websiteId, string languageCode, CancellationToken ct = default);
}
