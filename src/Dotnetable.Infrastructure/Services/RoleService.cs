using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly AppDbContext _context;

    public RoleService(AppDbContext context) => _context = context;

    public async Task<PagedResult<Role>> GetPagedAsync(GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Roles.AsNoTracking();

        if (query.GetSearch("RoleKey") is string key)
            q = q.Where(r => r.RoleKey.Contains(key));
        if (query.GetSearch("Description") is string desc)
            q = q.Where(r => r.Description.Contains(desc));
        if (query.GetSearch("Active") is string active && bool.TryParse(active, out var isActive))
            q = q.Where(r => r.Active == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Role.RoleID))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Role> { Items = items, TotalCount = total };
    }

    public async Task<Role?> GetByIdAsync(short id, CancellationToken ct = default) =>
        await _context.Roles.FindAsync([id], ct);

    public async Task<IReadOnlyList<Role>> GetAllActiveAsync(CancellationToken ct = default) =>
        await _context.Roles.AsNoTracking().Where(r => r.Active).OrderBy(r => r.RoleKey).ToListAsync(ct);

    public async Task<Role> CreateAsync(Role role, CancellationToken ct = default)
    {
        _context.Roles.Add(role);
        await _context.SaveChangesAsync(ct);
        return role;
    }

    public async Task UpdateAsync(Role role, CancellationToken ct = default)
    {
        _context.Roles.Update(role);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(short id, CancellationToken ct = default)
    {
        var role = await _context.Roles.FindAsync([id], ct);
        if (role is null) return;
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(ct);
    }
}
