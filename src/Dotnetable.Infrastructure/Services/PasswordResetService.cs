using System.Security.Cryptography;
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

    private readonly AppDbContext _context;
    private readonly IEmailService _email;
    private readonly IPasswordHasher<Member> _hasher;

    public PasswordResetService(AppDbContext context, IEmailService email, IPasswordHasher<Member> hasher)
    {
        _context = context;
        _email = email;
        _hasher = hasher;
    }

    public async Task<PasswordResetRequestResult> RequestResetAsync(
        string emailOrUsername, Func<string, string> resetUrlBuilder, CancellationToken ct = default)
    {
        var needle = emailOrUsername.Trim();
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Active && (m.Username == needle || m.Email == needle), ct);

        if (member is null)
            return PasswordResetRequestResult.MemberNotFound;

        if (!await _email.IsConfiguredAsync(ct))
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
        var body =
            $"<p>Hello {System.Net.WebUtility.HtmlEncode(member.Givenname)},</p>" +
            "<p>We received a request to reset your Dotnetable admin password. " +
            "Click the link below to choose a new password. This link expires in 30 minutes.</p>" +
            $"<p><a href=\"{resetUrl}\">{resetUrl}</a></p>" +
            "<p>If you did not request this, you can safely ignore this email.</p>";

        await _email.SendAsync(member.Email, "Reset your Dotnetable admin password", body, ct);
        return PasswordResetRequestResult.Sent;
    }

    public async Task<bool> IsKeyValidAsync(string key, CancellationToken ct = default) =>
        await FindValidAsync(key, ct) is not null;

    public async Task<bool> ResetPasswordAsync(string key, string newPassword, CancellationToken ct = default)
    {
        var entry = await FindValidAsync(key, ct);
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

    private async Task<MemberForgetPassword?> FindValidAsync(string key, CancellationToken ct)
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
