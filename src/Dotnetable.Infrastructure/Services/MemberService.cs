using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class MemberService : IMemberService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<Member> _hasher;

    public MemberService(AppDbContext context, IPasswordHasher<Member> hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public async Task<Member?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.Members.FindAsync([id], ct);

    public async Task<Member?> ValidateCredentialsAsync(string username, string password, CancellationToken ct = default)
    {
        var member = await _context.Members
            .Include(m => m.Policy)
                .ThenInclude(p => p.PolicyRoles)
                    .ThenInclude(pr => pr.Role)
            .FirstOrDefaultAsync(m => m.Username == username && m.Active, ct);

        if (member is null) return null;

        var result = _hasher.VerifyHashedPassword(member, member.Password, password);
        return result == PasswordVerificationResult.Failed ? null : member;
    }

    public async Task<int?> GetWebsiteIdByUsernameAsync(string username, CancellationToken ct = default) =>
        await _context.Members
            .Where(m => m.Username == username)
            .Select(m => (int?)m.WebsiteID)
            .FirstOrDefaultAsync(ct);

    public async Task<IEnumerable<Member>> GetByWebsiteAsync(int websiteId, CancellationToken ct = default) =>
        await _context.Members.Where(m => m.WebsiteID == websiteId).ToListAsync(ct);

    public async Task<IEnumerable<Member>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Members.ToListAsync(ct);

    public async Task<PagedResult<Member>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Members.AsNoTracking();

        if (websiteId is int wid)
            q = q.Where(m => m.WebsiteID == wid);

        if (query.GetSearch("Username") is string username)
            q = q.Where(m => m.Username.Contains(username));
        if (query.GetSearch("Email") is string email)
            q = q.Where(m => m.Email.Contains(email));
        if (query.GetSearch("Fullname") is string fullname)
            q = q.Where(m => (m.Givenname + " " + m.Surname).Contains(fullname));
        if (query.GetSearch("Active") is string active && bool.TryParse(active, out var isActive))
            q = q.Where(m => m.Active == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Member.MemberID))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Member> { Items = items, TotalCount = total };
    }

    public async Task SetActiveAsync(int id, bool active, CancellationToken ct = default)
    {
        await _context.Members.Where(m => m.MemberID == id)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.Active, active), ct);
    }

    public async Task<Member> CreateAsync(Member member, string plainPassword, CancellationToken ct = default)
    {
        if (member.HashKey == Guid.Empty) member.HashKey = Guid.NewGuid();
        member.Password = _hasher.HashPassword(member, plainPassword);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(ct);
        return member;
    }

    public async Task UpdateAsync(Member member, CancellationToken ct = default)
    {
        _context.Members.Update(member);
        await _context.SaveChangesAsync(ct);
    }

    public async Task ChangePasswordAsync(int memberId, string newPassword, CancellationToken ct = default)
    {
        var member = await _context.Members.FindAsync([memberId], ct)
            ?? throw new KeyNotFoundException($"Member {memberId} not found.");
        member.HashKey = Guid.NewGuid();
        member.Password = _hasher.HashPassword(member, newPassword);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var member = await _context.Members.FindAsync([id], ct);
        if (member is null) return;
        _context.Members.Remove(member);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetRoleKeysAsync(int memberId, CancellationToken ct = default) =>
        await _context.Members
            .Where(m => m.MemberID == memberId)
            .SelectMany(m => m.Policy.PolicyRoles)
            .Where(pr => pr.Active && pr.Role.Active)
            .Select(pr => pr.Role.RoleKey)
            .Distinct()
            .ToListAsync(ct);
}
