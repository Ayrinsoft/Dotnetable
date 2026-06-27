using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class PolicyService : IPolicyService
{
    private readonly AppDbContext _context;

    public PolicyService(AppDbContext context) => _context = context;

    public async Task<PagedResult<Policy>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Policies.AsNoTracking();

        if (websiteId is int wid)
            q = q.Where(p => p.WebsiteID == wid);

        if (query.GetSearch("Title") is string title)
            q = q.Where(p => p.Title.Contains(title));
        if (query.GetSearch("Active") is string active && bool.TryParse(active, out var isActive))
            q = q.Where(p => p.Active == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Policy.PolicyID))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Policy> { Items = items, TotalCount = total };
    }

    public async Task<Policy?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.Policies
            .Include(p => p.PolicyRoles)
            .FirstOrDefaultAsync(p => p.PolicyID == id, ct);

    public async Task<IReadOnlyList<Policy>> GetByWebsiteAsync(int websiteId, CancellationToken ct = default) =>
        await _context.Policies.AsNoTracking()
            .Where(p => p.WebsiteID == websiteId && p.Active)
            .OrderBy(p => p.Title)
            .ToListAsync(ct);

    public async Task<Policy?> GetDefaultMemberPolicyAsync(int websiteId, CancellationToken ct = default) =>
        await _context.Policies.AsNoTracking()
            .FirstOrDefaultAsync(p => p.WebsiteID == websiteId && p.Active && p.Title == DefaultPolicies.Users, ct);

    public async Task<Policy> CreateAsync(Policy policy, IEnumerable<short> roleIds, CancellationToken ct = default)
    {
        _context.Policies.Add(policy);
        await _context.SaveChangesAsync(ct);

        foreach (var roleId in roleIds.Distinct())
            _context.PolicyRoles.Add(new PolicyRole { PolicyID = policy.PolicyID, RoleID = roleId, Active = true });
        await _context.SaveChangesAsync(ct);

        return policy;
    }

    public async Task UpdateAsync(Policy policy, IEnumerable<short> roleIds, CancellationToken ct = default)
    {
        _context.Policies.Update(policy);

        var desired = roleIds.Distinct().ToHashSet();
        var existing = await _context.PolicyRoles.Where(pr => pr.PolicyID == policy.PolicyID).ToListAsync(ct);

        // Remove roles no longer selected.
        foreach (var pr in existing.Where(pr => !desired.Contains(pr.RoleID)))
            _context.PolicyRoles.Remove(pr);

        // Add newly selected roles.
        var existingIds = existing.Select(pr => pr.RoleID).ToHashSet();
        foreach (var roleId in desired.Where(id => !existingIds.Contains(id)))
            _context.PolicyRoles.Add(new PolicyRole { PolicyID = policy.PolicyID, RoleID = roleId, Active = true });

        await _context.SaveChangesAsync(ct);
    }

    public async Task SetActiveAsync(int id, bool active, CancellationToken ct = default)
    {
        await _context.Policies.Where(p => p.PolicyID == id)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Active, active), ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var policy = await _context.Policies.FindAsync([id], ct);
        if (policy is null) return;
        _context.Policies.Remove(policy);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Role>> GetAssignableRolesAsync(int memberId, bool isMaster, CancellationToken ct = default)
    {
        // The admin policy editor only deals with admin-panel permissions; client permissions
        // (Category 1) belong to website customers and are managed elsewhere.
        const byte adminCategory = (byte)RoleCategory.Admin;

        if (isMaster)
            return await _context.Roles.AsNoTracking()
                .Where(r => r.Active && r.Category == adminCategory)
                .OrderBy(r => r.RoleKey)
                .ToListAsync(ct);

        var heldRoleIds = await _context.Members
            .Where(m => m.MemberID == memberId)
            .SelectMany(m => m.Policy.PolicyRoles)
            .Where(pr => pr.Active && pr.Role.Active)
            .Select(pr => pr.RoleID)
            .Distinct()
            .ToListAsync(ct);

        return await _context.Roles.AsNoTracking()
            .Where(r => r.Active && r.Category == adminCategory && heldRoleIds.Contains(r.RoleID))
            .OrderBy(r => r.RoleKey)
            .ToListAsync(ct);
    }
}
