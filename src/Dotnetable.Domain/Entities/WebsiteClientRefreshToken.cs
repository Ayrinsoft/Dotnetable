using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// A rotating refresh token for a signed-in website customer. The API issues one alongside every
/// access token; presenting it mints a new pair and revokes this row (rotation), so a stolen token
/// is usable at most once and its reuse is detectable.
/// </summary>
public partial class WebsiteClientRefreshToken
{
    public int WebsiteClientRefreshTokenID { get; set; }

    public int WebsiteClientID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>SHA-256 of the token handed to the client. The raw value is never stored.</summary>
    public string TokenHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    /// <summary>Set when the token was exchanged or explicitly revoked (sign-out, password reset).</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>The token this one was rotated into, so a replayed old token reveals the whole chain.</summary>
    public int? ReplacedByTokenID { get; set; }

    public string? CreatedByIp { get; set; }

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;
}
