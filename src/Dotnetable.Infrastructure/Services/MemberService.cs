using System.Security.Cryptography;
using System.Text;
using Dotnetable.Application.Security;
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
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IPasswordHasher<Member> _hasher;

    public MemberService(IDbContextFactory<AppDbContext> contextFactory, IPasswordHasher<Member> hasher)
    {
        _contextFactory = contextFactory;
        _hasher = hasher;
    }

    public async Task<Member?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.Members.FindAsync([id], ct);
    }

    /// <summary>Wrong passwords tolerated before the account is locked.</summary>
    private const int MaxLoginAttempts = 5;

    /// <summary>How long an account stays locked after <see cref="MaxLoginAttempts"/> failures.</summary>
    private static readonly TimeSpan LockoutWindow = TimeSpan.FromMinutes(15);

    /// <summary>Recovery codes handed out when a member enrols a second factor.</summary>
    private const int RecoveryCodeCount = 10;

    [Obsolete("Use ValidateSignInAsync.")]
    public async Task<Member?> ValidateCredentialsAsync(string username, string password, CancellationToken ct = default)
    {
        var result = await ValidateSignInAsync(username, password, ct);
        return result.Status == MemberSignInStatus.Success ? result.Member : null;
    }

    public async Task<MemberSignInResult> ValidateSignInAsync(string username, string password, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var member = await _context.Members
            .Include(m => m.Policy)
                .ThenInclude(p => p.PolicyRoles)
                    .ThenInclude(pr => pr.Role)
            .Include(m => m.Vendor)
            .FirstOrDefaultAsync(m => m.Username == username && m.Active, ct);

        if (member is null) return MemberSignInResult.Invalid;

        if (member.LockoutEndUtc is DateTime until && until > DateTime.UtcNow)
            return new MemberSignInResult(MemberSignInStatus.LockedOut, null, until);

        var result = _hasher.VerifyHashedPassword(member, member.Password, password);
        if (result == PasswordVerificationResult.Failed)
        {
            member.FailedLoginCount++;
            if (member.FailedLoginCount >= MaxLoginAttempts)
            {
                member.LockoutEndUtc = DateTime.UtcNow.Add(LockoutWindow);
                member.FailedLoginCount = 0;
            }
            await _context.SaveChangesAsync(ct);

            return member.LockoutEndUtc is DateTime locked
                ? new MemberSignInResult(MemberSignInStatus.LockedOut, null, locked)
                : MemberSignInResult.Invalid;
        }

        if (member.FailedLoginCount != 0 || member.LockoutEndUtc is not null)
        {
            member.FailedLoginCount = 0;
            member.LockoutEndUtc = null;
            await _context.SaveChangesAsync(ct);
        }

        // A hash written under older Identity parameters is upgraded on the next successful sign-in,
        // the only moment the plaintext is available to rehash from.
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            member.Password = _hasher.HashPassword(member, password);
            await _context.SaveChangesAsync(ct);
        }

        // The password is correct but not sufficient — the caller must challenge for the code and
        // must not issue an authentication cookie yet.
        if (member.TwoFactorEnabled && !string.IsNullOrWhiteSpace(member.TwoFactorSecret))
            return new MemberSignInResult(MemberSignInStatus.TwoFactorRequired, member);

        return new MemberSignInResult(MemberSignInStatus.Success, member);
    }

    public async Task<MemberSignInResult> VerifyTwoFactorAsync(int memberId, string code, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var member = await _context.Members
            .Include(m => m.Policy)
                .ThenInclude(p => p.PolicyRoles)
                    .ThenInclude(pr => pr.Role)
            .Include(m => m.Vendor)
            .FirstOrDefaultAsync(m => m.MemberID == memberId && m.Active, ct);

        if (member is null || !member.TwoFactorEnabled || string.IsNullOrWhiteSpace(member.TwoFactorSecret))
            return MemberSignInResult.Invalid;

        if (member.LockoutEndUtc is DateTime until && until > DateTime.UtcNow)
            return new MemberSignInResult(MemberSignInStatus.LockedOut, null, until);

        if (Totp.Verify(member.TwoFactorSecret, code))
        {
            member.FailedLoginCount = 0;
            await _context.SaveChangesAsync(ct);
            return new MemberSignInResult(MemberSignInStatus.Success, member);
        }

        // A recovery code is single-use: consuming it removes it from the stored set, so a code read
        // off a printout that has already been used is worthless.
        if (TryConsumeRecoveryCode(member, code))
        {
            member.FailedLoginCount = 0;
            await _context.SaveChangesAsync(ct);
            return new MemberSignInResult(MemberSignInStatus.Success, member);
        }

        // A six-digit code is as guessable as a weak password, so failures count toward the same
        // lockout rather than being unlimited.
        member.FailedLoginCount++;
        if (member.FailedLoginCount >= MaxLoginAttempts)
        {
            member.LockoutEndUtc = DateTime.UtcNow.Add(LockoutWindow);
            member.FailedLoginCount = 0;
        }
        await _context.SaveChangesAsync(ct);

        return member.LockoutEndUtc is DateTime locked
            ? new MemberSignInResult(MemberSignInStatus.LockedOut, null, locked)
            : new MemberSignInResult(MemberSignInStatus.InvalidTwoFactorCode);
    }

    public TwoFactorEnrolment BeginTwoFactorEnrolment(Member member, string issuer)
    {
        var secret = Totp.GenerateSecret();
        return new TwoFactorEnrolment(secret, Totp.BuildProvisioningUri(secret, issuer, member.Username));
    }

    public async Task<IReadOnlyList<string>?> ConfirmTwoFactorAsync(
        int memberId, string secret, string code, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (!Totp.Verify(secret, code)) return null;

        var member = await _context.Members.FirstOrDefaultAsync(m => m.MemberID == memberId, ct);
        if (member is null) return null;

        var recoveryCodes = Enumerable.Range(0, RecoveryCodeCount).Select(_ => NewRecoveryCode()).ToList();

        member.TwoFactorSecret = secret;
        member.TwoFactorEnabled = true;
        // Stored as hashes: a database read must not hand over a working bypass for every admin.
        member.TwoFactorRecoveryCodes = string.Join('\n', recoveryCodes.Select(c => HashRecoveryCode(c)));
        await _context.SaveChangesAsync(ct);

        return recoveryCodes;
    }

    public async Task DisableTwoFactorAsync(int memberId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var member = await _context.Members.FirstOrDefaultAsync(m => m.MemberID == memberId, ct);
        if (member is null) return;

        member.TwoFactorEnabled = false;
        member.TwoFactorSecret = null;
        member.TwoFactorRecoveryCodes = null;
        await _context.SaveChangesAsync(ct);
    }

    private static bool TryConsumeRecoveryCode(Member member, string code)
    {
        if (string.IsNullOrWhiteSpace(member.TwoFactorRecoveryCodes)) return false;

        var normalized = code.Replace(" ", "").Replace("-", "").ToUpperInvariant();
        var target = HashRecoveryCode(normalized);

        var remaining = member.TwoFactorRecoveryCodes
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var matched = false;
        for (var i = remaining.Count - 1; i >= 0; i--)
        {
            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(remaining[i]), Encoding.ASCII.GetBytes(target)))
                continue;

            remaining.RemoveAt(i);
            matched = true;
            break;
        }

        if (!matched) return false;

        member.TwoFactorRecoveryCodes = remaining.Count == 0 ? null : string.Join('\n', remaining);
        return true;
    }

    /// <summary>Ten hex characters from the CSPRNG, grouped for legibility when written down.</summary>
    private static string NewRecoveryCode()
    {
        var raw = Convert.ToHexString(RandomNumberGenerator.GetBytes(5));
        return $"{raw[..5]}-{raw[5..]}";
    }

    private static string HashRecoveryCode(string code) =>
        Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(code.Replace(" ", "").Replace("-", "").ToUpperInvariant())));

    public async Task<bool> ExistsAsync(string username, string email, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.Members.AnyAsync(m => m.Username == username || m.Email == email, ct);
    }

    public async Task<int?> GetWebsiteIdByUsernameAsync(string username, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.Members
            .Where(m => m.Username == username)
            .Select(m => (int?)m.WebsiteID)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<Member>> GetByWebsiteAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.Members.Where(m => m.WebsiteID == websiteId).ToListAsync(ct);
    }

    public async Task<IEnumerable<Member>> GetAllAsync(CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.Members.ToListAsync(ct);
    }

    public async Task<PagedResult<Member>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

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
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        await _context.Members.Where(m => m.MemberID == id)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.Active, active), ct);
    }

    public async Task<Member> CreateAsync(Member member, string plainPassword, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (member.HashKey == Guid.Empty) member.HashKey = Guid.NewGuid();
        member.Password = _hasher.HashPassword(member, plainPassword);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(ct);
        return member;
    }

    public async Task UpdateAsync(Member member, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        _context.Members.Update(member);
        await _context.SaveChangesAsync(ct);
    }

    public async Task ChangePasswordAsync(int memberId, string newPassword, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var member = await _context.Members.FindAsync([memberId], ct)
            ?? throw new KeyNotFoundException($"Member {memberId} not found.");

        // Enforced here rather than in the page, so every caller — the panel form, seeding, any
        // future API — gets the same rule. An admin password guards every order in the shop.
        var policy = PasswordPolicy.ValidateAdmin(newPassword, member.Username, member.Email);
        if (!policy.Ok) throw new InvalidOperationException(policy.Error);

        member.HashKey = Guid.NewGuid();
        member.Password = _hasher.HashPassword(member, newPassword);
        // A password change clears an outstanding lockout: the owner has proved control.
        member.FailedLoginCount = 0;
        member.LockoutEndUtc = null;
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var member = await _context.Members.FindAsync([id], ct);
        if (member is null) return;
        _context.Members.Remove(member);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetRoleKeysAsync(int memberId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.Members
            .Where(m => m.MemberID == memberId)
            .SelectMany(m => m.Policy.PolicyRoles)
            .Where(pr => pr.Active && pr.Role.Active)
            .Select(pr => pr.Role.RoleKey)
            .Distinct()
            .ToListAsync(ct);
    }
}
