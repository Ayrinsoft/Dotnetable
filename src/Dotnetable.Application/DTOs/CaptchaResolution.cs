namespace Dotnetable.Application.DTOs;

/// <summary>Which captcha a specific website's public form should use, resolved from its
/// <see cref="Domain.Entities.WebsiteCaptchaSetting"/> (or the math fallback when unset).</summary>
public sealed class CaptchaResolution
{
    public bool UseTurnstile { get; set; }

    public string? TurnstileSiteKey { get; set; }

    public string? TurnstileSecretKey { get; set; }
}
