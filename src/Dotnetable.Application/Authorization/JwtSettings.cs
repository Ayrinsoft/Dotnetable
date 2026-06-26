namespace Dotnetable.Application.Authorization;

/// <summary>JWT bearer settings bound from the "Jwt" configuration section. Used to issue and validate API tokens.</summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Dotnetable";
    public string Audience { get; set; } = "Dotnetable.Clients";

    /// <summary>HMAC-SHA256 signing key. Must be at least 32 bytes; supply via configuration/secret.</summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>Access-token lifetime in minutes.</summary>
    public int AccessTokenMinutes { get; set; } = 120;
}
