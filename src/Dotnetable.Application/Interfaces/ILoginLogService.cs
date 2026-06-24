using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface ILoginLogService
{
    /// <summary>Server-side paged/sorted/searched login attempts. <paramref name="websiteId"/> null = all websites (master only).</summary>
    Task<PagedResult<LoginTry>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    /// <summary>Records a single login attempt.</summary>
    Task RecordAsync(string username, int websiteId, bool success, string ip, CancellationToken ct = default);
}
