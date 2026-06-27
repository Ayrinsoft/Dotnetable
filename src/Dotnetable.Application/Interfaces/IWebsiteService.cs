using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

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
    Task<Website> CreateAsync(Website website, CancellationToken ct = default);
    Task UpdateAsync(Website website, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
