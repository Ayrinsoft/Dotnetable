using System.Security.Cryptography;
using System.Text;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

/// <inheritdoc cref="IRefreshTokenService" />
public sealed class RefreshTokenService : IRefreshTokenService
{
    /// <summary>How long a refresh token stays usable. Each exchange restarts the clock.</summary>
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);

    /// <summary>
    /// How long a spent/revoked row is kept. Rotation only detects theft while the old row still
    /// exists, so the purge must lag well behind the lifetime rather than delete on use.
    /// </summary>
    private static readonly TimeSpan RetentionAfterExpiry = TimeSpan.FromDays(30);

    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(IDbContextFactory<AppDbContext> contextFactory, ILogger<RefreshTokenService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<RefreshTokenResult> IssueAsync(WebsiteClient client, string? ip, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var (raw, row) = NewToken(client.WebsiteClientID, client.WebsiteID, ip);
        context.WebsiteClientRefreshTokens.Add(row);
        await context.SaveChangesAsync(ct);
        return new RefreshTokenResult(raw, row.ExpiresAt);
    }

    public async Task<RefreshExchangeResult> ExchangeAsync(
        int websiteId, string rawToken, string? ip, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken)) return RefreshExchangeResult.Invalid;

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var hash = Hash(rawToken);
        var existing = await context.WebsiteClientRefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash && t.WebsiteID == websiteId, ct);

        if (existing is null) return RefreshExchangeResult.Invalid;

        // A token that was already spent is either a replay of a stolen copy or a duplicate request
        // from a racing client. Both are handled the same way: assume compromise and cut the whole
        // family, because the alternative — letting one of the two through — is what makes stolen
        // refresh tokens durable.
        if (existing.RevokedAt is not null)
        {
            await RevokeFamilyAsync(context, existing.WebsiteClientID, ct);
            _logger.LogWarning(
                "Refresh-token reuse detected for client {ClientId}; all sessions revoked.",
                existing.WebsiteClientID);
            return RefreshExchangeResult.Reused;
        }

        if (existing.ExpiresAt <= DateTime.UtcNow)
            return RefreshExchangeResult.Invalid;

        var client = await context.WebsiteClients
            .FirstOrDefaultAsync(c => c.WebsiteClientID == existing.WebsiteClientID, ct);

        // Deactivated or locked-out accounts must not be able to refresh their way back in.
        if (client is null || !client.Active ||
            (client.LockoutEndUtc is DateTime until && until > DateTime.UtcNow))
            return RefreshExchangeResult.Invalid;

        var (raw, replacement) = NewToken(client.WebsiteClientID, client.WebsiteID, ip);
        context.WebsiteClientRefreshTokens.Add(replacement);
        await context.SaveChangesAsync(ct);

        existing.RevokedAt = DateTime.UtcNow;
        existing.ReplacedByTokenID = replacement.WebsiteClientRefreshTokenID;
        await context.SaveChangesAsync(ct);

        return new RefreshExchangeResult(client, new RefreshTokenResult(raw, replacement.ExpiresAt));
    }

    public async Task RevokeAsync(string rawToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken)) return;

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var hash = Hash(rawToken);
        await context.WebsiteClientRefreshTokens
            .Where(t => t.TokenHash == hash && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, (DateTime?)DateTime.UtcNow), ct);
    }

    public async Task RevokeAllForClientAsync(int clientId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await RevokeFamilyAsync(context, clientId, ct);
    }

    public async Task<int> PurgeExpiredAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var cutoff = DateTime.UtcNow - RetentionAfterExpiry;
        return await context.WebsiteClientRefreshTokens
            .Where(t => t.ExpiresAt < cutoff)
            .ExecuteDeleteAsync(ct);
    }

    private static Task RevokeFamilyAsync(AppDbContext context, int clientId, CancellationToken ct) =>
        context.WebsiteClientRefreshTokens
            .Where(t => t.WebsiteClientID == clientId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, (DateTime?)DateTime.UtcNow), ct);

    private static (string Raw, WebsiteClientRefreshToken Row) NewToken(int clientId, int websiteId, string? ip)
    {
        // 256 bits from the CSPRNG: unguessable, and the only copy the server keeps is the hash, so a
        // database leak does not hand over live sessions.
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        var row = new WebsiteClientRefreshToken
        {
            WebsiteClientID = clientId,
            WebsiteID = websiteId,
            TokenHash = Hash(raw),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(Lifetime),
            CreatedByIp = Truncate(ip, 45),
        };
        return (raw, row);
    }

    /// <summary>SHA-256, hex-encoded — a 64-char fixed-width value the unique index can key on.</summary>
    private static string Hash(string raw) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();

    private static string? Truncate(string? value, int max) =>
        string.IsNullOrEmpty(value) ? null : (value.Length <= max ? value : value[..max]);
}
