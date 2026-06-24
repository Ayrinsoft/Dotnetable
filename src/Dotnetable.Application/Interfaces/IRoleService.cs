using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Global role (permission key) definitions, managed only by the master website.</summary>
public interface IRoleService
{
    Task<PagedResult<Role>> GetPagedAsync(GridQuery query, CancellationToken ct = default);
    Task<Role?> GetByIdAsync(short id, CancellationToken ct = default);
    Task<IReadOnlyList<Role>> GetAllActiveAsync(CancellationToken ct = default);
    Task<Role> CreateAsync(Role role, CancellationToken ct = default);
    Task UpdateAsync(Role role, CancellationToken ct = default);
    Task DeleteAsync(short id, CancellationToken ct = default);
}
