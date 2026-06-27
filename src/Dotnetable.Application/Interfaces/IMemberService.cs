using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IMemberService
{
    Task<Member?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Server-side paged/sorted/searched members. <paramref name="websiteId"/> null = all websites (master only).</summary>
    Task<PagedResult<Member>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    /// <summary>Validates credentials and returns the member with its Policy and Roles loaded, or null.</summary>
    Task<Member?> ValidateCredentialsAsync(string username, string password, CancellationToken ct = default);

    /// <summary>WebsiteID of the member with this username (any status), or null if no such member — used to attribute failed logins.</summary>
    Task<int?> GetWebsiteIdByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>True when a member already exists with this username or email (used to reject duplicate registrations).</summary>
    Task<bool> ExistsAsync(string username, string email, CancellationToken ct = default);

    Task<IEnumerable<Member>> GetByWebsiteAsync(int websiteId, CancellationToken ct = default);

    /// <summary>All members. Only meaningful for master-website administrators.</summary>
    Task<IEnumerable<Member>> GetAllAsync(CancellationToken ct = default);

    Task SetActiveAsync(int id, bool active, CancellationToken ct = default);
    Task<Member> CreateAsync(Member member, string plainPassword, CancellationToken ct = default);
    Task UpdateAsync(Member member, CancellationToken ct = default);
    Task ChangePasswordAsync(int memberId, string newPassword, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Permission keys (Role.RoleKey) granted to the member through its policy.</summary>
    Task<IReadOnlyList<string>> GetRoleKeysAsync(int memberId, CancellationToken ct = default);
}
