using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

/// <summary>Admin-side management of website customers (clients). Customers have no policy — only a level.</summary>
public interface IWebsiteClientService
{
    Task<WebsiteClient?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Server-side paged/sorted/searched customers. <paramref name="websiteId"/> null = all websites (master only).</summary>
    Task<PagedResult<WebsiteClient>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    Task SetActiveAsync(int id, bool active, CancellationToken ct = default);
    Task SetLevelAsync(int id, ClientLevel level, CancellationToken ct = default);

    /// <summary>Updates the editable profile/level fields of a customer.</summary>
    Task UpdateAsync(WebsiteClient client, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
