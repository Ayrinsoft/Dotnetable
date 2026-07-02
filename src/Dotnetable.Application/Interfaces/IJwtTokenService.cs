using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Issues signed JWT access tokens for authenticated members (used by the API / website clients).</summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Creates a signed access token carrying the member's identity and role claims. The member must
    /// have its Policy → PolicyRoles → Role graph loaded so role claims are populated.
    /// </summary>
    JwtTokenResult CreateToken(Member member);

    /// <summary>
    /// Creates a signed access token for a website customer, carrying the fixed client permission
    /// keys and the customer's loyalty level.
    /// </summary>
    JwtTokenResult CreateToken(WebsiteClient client);
}

/// <summary>A freshly issued access token and its absolute expiry (UTC).</summary>
public sealed record JwtTokenResult(string AccessToken, DateTime ExpiresAtUtc);
