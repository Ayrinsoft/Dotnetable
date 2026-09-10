using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface ILoginLogService
{
    /// <summary>Server-side paged/sorted/searched login attempts. <paramref name="websiteId"/> null = all websites (master only).</summary>
    Task<PagedResult<LoginTry>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    /// <summary>
    /// Records a single login attempt. <paramref name="websiteId"/> null (or ≤ 0) means the attempt
    /// is not attributed to a website — used for unknown usernames on the admin panel. Never persist 0:
    /// there is no such Website row and the FK would reject it.
    /// </summary>
    Task RecordAsync(string username, int? websiteId, bool success, string ip, CancellationToken ct = default);
}
