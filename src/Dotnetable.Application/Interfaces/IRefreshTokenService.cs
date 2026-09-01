using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Rotating refresh tokens for website customers.
///
/// <para>Access tokens are short-lived and are not revocable once issued, so they must stay short —
/// which used to mean the customer was signed out every two hours. A refresh token closes that gap:
/// it is long-lived, stored only as a hash, and single-use. Presenting one mints a new access/refresh
/// pair and immediately revokes the presented row, so a token copied off a device stops working the
/// moment the real owner uses theirs. Presenting an already-revoked token is treated as theft and
/// revokes the customer's whole family of tokens rather than just failing.</para>
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>Mints a refresh token for a customer and returns the raw value (stored only as a hash).</summary>
    Task<RefreshTokenResult> IssueAsync(WebsiteClient client, string? ip, CancellationToken ct = default);

    /// <summary>
    /// Exchanges a refresh token for a new pair. Returns the customer when the token was live; null
    /// when it was unknown, expired or already used — in which case any surviving tokens for that
    /// customer are revoked too.
    /// </summary>
    Task<RefreshExchangeResult> ExchangeAsync(int websiteId, string rawToken, string? ip, CancellationToken ct = default);

    /// <summary>Revokes a single token (sign-out). Silently succeeds when it is unknown.</summary>
    Task RevokeAsync(string rawToken, CancellationToken ct = default);

    /// <summary>Revokes every live token for a customer (password change, "sign out everywhere").</summary>
    Task RevokeAllForClientAsync(int clientId, CancellationToken ct = default);

    /// <summary>Deletes rows that expired or were revoked long enough ago to be useless. Returns the count.</summary>
    Task<int> PurgeExpiredAsync(CancellationToken ct = default);
}

/// <summary>A freshly minted refresh token. <paramref name="Token"/> is shown to the client exactly once.</summary>
public sealed record RefreshTokenResult(string Token, DateTime ExpiresAtUtc);

/// <summary>Outcome of a refresh exchange.</summary>
/// <param name="Client">The customer, when the exchange succeeded.</param>
/// <param name="Refresh">The replacement refresh token, when the exchange succeeded.</param>
/// <param name="ReuseDetected">True when a revoked token was replayed, which invalidated the family.</param>
public sealed record RefreshExchangeResult(WebsiteClient? Client, RefreshTokenResult? Refresh, bool ReuseDetected = false)
{
    public bool Success => Client is not null && Refresh is not null;

    public static readonly RefreshExchangeResult Invalid = new(null, null);
    public static readonly RefreshExchangeResult Reused = new(null, null, true);
}
