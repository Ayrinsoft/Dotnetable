using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

/// <summary>Result of an attempt to create or update a customer from the admin panel.</summary>
public enum ClientSaveResult
{
    Success,
    /// <summary>The email or cellphone is already used by another customer on the same website.</summary>
    Duplicate,
}

/// <summary>Admin-side management of website customers (clients). Customers have no policy — only a level.</summary>
public interface IWebsiteClientService
{
    Task<WebsiteClient?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Server-side paged/sorted/searched customers. <paramref name="websiteId"/> null = all websites (master only).</summary>
    Task<PagedResult<WebsiteClient>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    Task SetActiveAsync(int id, bool active, CancellationToken ct = default);
    Task SetLevelAsync(int id, ClientLevel level, CancellationToken ct = default);

    /// <summary>Creates a customer from the admin panel with the given plaintext password. Fails with
    /// <see cref="ClientSaveResult.Duplicate"/> when the email or cellphone is already taken on the website.</summary>
    Task<(ClientSaveResult Result, WebsiteClient Client)> CreateAsync(WebsiteClient client, string password, CancellationToken ct = default);

    /// <summary>Updates the editable profile/level/email/cellphone fields of a customer. Fails with
    /// <see cref="ClientSaveResult.Duplicate"/> when the email or cellphone is already taken by another
    /// customer on the same website.</summary>
    Task<ClientSaveResult> UpdateAsync(WebsiteClient client, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
