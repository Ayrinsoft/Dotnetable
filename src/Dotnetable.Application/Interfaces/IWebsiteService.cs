using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

public interface IWebsiteService
{
    Task<Website?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Website?> GetByAddressAsync(string websiteAddress, CancellationToken ct = default);

    /// <summary>Resolves a website by its per-site key (<see cref="Website.AuthCode"/>), or null if no match.</summary>
    Task<Website?> GetByAuthCodeAsync(Guid authCode, CancellationToken ct = default);

    Task<IEnumerable<Website>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Server-side paged/sorted/searched websites.</summary>
    Task<PagedResult<Website>> GetPagedAsync(GridQuery query, CancellationToken ct = default);
    Task SetActiveAsync(int id, bool active, CancellationToken ct = default);

    /// <summary>Creates the website, then seeds its <see cref="WebsiteFeature"/> rows from
    /// <see cref="WebsiteTypeExtensions.GetDefaultFeatures"/> for the chosen <see cref="Website.WebsiteType"/>.</summary>
    Task<Website> CreateAsync(Website website, CancellationToken ct = default);
    Task UpdateAsync(Website website, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>All feature toggles recorded for a website (only rows that have ever been set — see <see cref="WebsiteFeature"/>).</summary>
    Task<IEnumerable<WebsiteFeature>> GetFeaturesAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Enables/disables a single feature for a website, inserting the row if it doesn't exist yet.</summary>
    Task SetFeatureAsync(int websiteId, WebsiteFeatureKey featureKey, bool enabled, CancellationToken ct = default);
}
