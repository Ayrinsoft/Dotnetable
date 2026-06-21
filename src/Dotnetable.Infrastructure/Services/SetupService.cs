using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class SetupService : ISetupService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<Member> _hasher;

    public SetupService(AppDbContext context, IPasswordHasher<Member> hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public async Task<bool> IsSetupCompletedAsync(CancellationToken ct = default) =>
        await _context.Websites.AnyAsync(ct);

    public async Task CompleteSetupAsync(SetupRequest request, CancellationToken ct = default)
    {
        if (await IsSetupCompletedAsync(ct))
            throw new InvalidOperationException("Setup has already been completed.");

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        // 1. Master website. As the first row inserted into an empty table it receives WebsiteID 1.
        var website = new Website
        {
            TradeName = request.TradeName,
            BrandName = request.BrandName,
            WebsiteAddress = request.WebsiteAddress,
            Manager = request.Manager,
            Mobile = request.Mobile,
            Email = request.WebsiteEmail,
            DefaultLanguageCode = request.DefaultLanguageCode,
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            AuthCode = Guid.NewGuid(),
            Active = true,
            AllowAllIP = true,
            WebsiteType = 0,
        };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync(ct);

        // 2. Seed permission roles.
        var roles = RoleKeys.All
            .Select(key => new Role { RoleKey = key, Description = key, Active = true })
            .ToList();
        _context.Roles.AddRange(roles);
        await _context.SaveChangesAsync(ct);

        // 3. Administrator policy with full access (every seeded role).
        var policy = new Policy { Title = "Administrators", Active = true };
        _context.Policies.Add(policy);
        await _context.SaveChangesAsync(ct);

        _context.PolicyRoles.AddRange(roles.Select(r => new PolicyRole
        {
            PolicyID = policy.PolicyID,
            RoleID = r.RoleID,
            Active = true,
        }));
        await _context.SaveChangesAsync(ct);

        // 4. First administrator member, bound to the master website.
        var member = new Member
        {
            WebsiteID = website.WebsiteID,
            PolicyID = policy.PolicyID,
            Username = request.Username,
            Email = request.Email,
            Givenname = request.Givenname,
            Surname = request.Surname,
            CellphoneNumber = string.Empty,
            CountryCode = string.Empty,
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            HashKey = Guid.NewGuid(),
            Active = true,
        };
        member.Password = _hasher.HashPassword(member, request.Password);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);
    }
}
