using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IPolicyService
{
    /// <summary>Server-side paged/sorted/searched access levels. <paramref name="websiteId"/> null = all websites (master only).</summary>
    Task<PagedResult<Policy>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    /// <summary>Access level with its <see cref="Policy.PolicyRoles"/> loaded, or null.</summary>
    Task<Policy?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Active access levels belonging to a website (for member assignment dropdowns).</summary>
    Task<IReadOnlyList<Policy>> GetByWebsiteAsync(int websiteId, CancellationToken ct = default);

    /// <summary>The active access level a website assigns to self-registered customers (the "Users" policy), or null.</summary>
    Task<Policy?> GetDefaultMemberPolicyAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Creates the access level and its active <see cref="PolicyRole"/> rows for the given roles.</summary>
    Task<Policy> CreateAsync(Policy policy, IEnumerable<short> roleIds, CancellationToken ct = default);

    /// <summary>Updates the access level and reconciles its <see cref="PolicyRole"/> rows to the given roles.</summary>
    Task UpdateAsync(Policy policy, IEnumerable<short> roleIds, CancellationToken ct = default);

    Task SetActiveAsync(int id, bool active, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Roles a member is allowed to grant: every active role for master, otherwise only the roles the member already holds.</summary>
    Task<IReadOnlyList<Role>> GetAssignableRolesAsync(int memberId, bool isMaster, CancellationToken ct = default);
}
