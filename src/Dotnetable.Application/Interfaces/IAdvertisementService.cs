using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Sponsored text links (keyword + URL) scoped to a single <see cref="Website"/>. The admin panel
/// uses the management methods directly; the public website consumes the read projections through
/// the API, filtered by <see cref="AdvertisementLocation"/> and language.
/// </summary>
public interface IAdvertisementService
{
    Task<List<Advertisement>> GetAllAsync(int? websiteId, CancellationToken ct = default);

    /// <summary>Paged / sorted / filtered advertisements for the admin grid.</summary>
    Task<PagedResult<Advertisement>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    Task<Advertisement?> GetByIdAsync(int advertisementId, CancellationToken ct = default);
    Task<Advertisement> CreateAsync(Advertisement advertisement, CancellationToken ct = default);
    Task UpdateAsync(Advertisement advertisement, CancellationToken ct = default);
    Task DeleteAsync(int advertisementId, CancellationToken ct = default);

    Task<List<AdvertisementTranslation>> GetTranslationsAsync(int advertisementId, CancellationToken ct = default);

    /// <summary>Replaces the advertisement's translations with the supplied language→(keyword, url) map
    /// (blank keywords remove that language's translation).</summary>
    Task SetTranslationsAsync(int advertisementId, IReadOnlyDictionary<string, (string Keyword, string? Url)> byLanguage, CancellationToken ct = default);

    /// <summary>Active advertisements assigned to a location, already localized and ordered.</summary>
    Task<List<AdvertisementDto>> GetByLocationAsync(int websiteId, AdvertisementLocation location, string? languageCode = null, CancellationToken ct = default);
}
