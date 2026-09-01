using System.Security.Cryptography;
using Dotnetable.Application.Email;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Forgot-password flow: issues a short, single-use key (stored in <c>MemberForgetPassword</c>),
/// emails a reset link, and applies a new password when a valid key is presented.
/// </summary>
public class PasswordResetService : IPasswordResetService
{
    // ForgetKey is an 8-char column; avoid easily confused characters (0/O, 1/I).
    private const string KeyAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private static readonly TimeSpan KeyLifetime = TimeSpan.FromMinutes(30);

    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IEmailService _email;
    private readonly IPasswordHasher<Member> _hasher;

    public PasswordResetService(IDbContextFactory<AppDbContext> contextFactory, IEmailService email, IPasswordHasher<Member> hasher)
    {
        _contextFactory = contextFactory;
        _email = email;
        _hasher = hasher;
    }

    public async Task<PasswordResetRequestResult> RequestResetAsync(
        string emailOrUsername, Func<string, string> resetUrlBuilder, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var needle = emailOrUsername.Trim();
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Active && (m.Username == needle || m.Email == needle), ct);

        if (member is null)
            return PasswordResetRequestResult.MemberNotFound;

        if (!await _email.IsConfiguredAsync(member.WebsiteID, ct))
            return PasswordResetRequestResult.EmailNotConfigured;

        var key = GenerateKey();
        _context.MemberForgetPasswords.Add(new MemberForgetPassword
        {
            MemberID = member.MemberID,
            ForgetKey = key,
            LogTime = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync(ct);

        var resetUrl = resetUrlBuilder(key);
        await _email.SendTemplateAsync(member.WebsiteID, EmailTemplateKeys.AdminForgotPassword, member.Email,
            new Dictionary<string, string> { ["Name"] = member.Givenname, ["ResetUrl"] = resetUrl },
            languageCode: null, ct);
        return PasswordResetRequestResult.Sent;
    }

    public async Task<bool> IsKeyValidAsync(string key, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await FindValidAsync(_context, key, ct) is not null;
    }

    public async Task<bool> ResetPasswordAsync(string key, string newPassword, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var entry = await FindValidAsync(_context, key, ct);
        if (entry is null) return false;

        var member = await _context.Members.FirstOrDefaultAsync(m => m.MemberID == entry.MemberID, ct);
        if (member is null) return false;

        member.HashKey = Guid.NewGuid();
        member.Password = _hasher.HashPassword(member, newPassword);

        // Invalidate every outstanding key for this member so the link can't be reused.
        var stale = _context.MemberForgetPasswords.Where(f => f.MemberID == member.MemberID);
        _context.MemberForgetPasswords.RemoveRange(stale);

        await _context.SaveChangesAsync(ct);
        return true;
    }

    private async Task<MemberForgetPassword?> FindValidAsync(AppDbContext _context, string key, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        var normalized = key.Trim().ToUpperInvariant();
        var cutoff = DateTime.UtcNow - KeyLifetime;

        return await _context.MemberForgetPasswords
            .Where(f => f.ForgetKey == normalized && f.LogTime >= cutoff)
            .OrderByDescending(f => f.LogTime)
            .FirstOrDefaultAsync(ct);
    }

    private static string GenerateKey()
    {
        Span<char> chars = stackalloc char[8];
        for (int i = 0; i < chars.Length; i++)
            chars[i] = KeyAlphabet[RandomNumberGenerator.GetInt32(KeyAlphabet.Length)];
        return new string(chars);
    }
}
