namespace Dotnetable.Application.DTOs;

/// <summary>Captcha challenge served to a public front-end (Web/React) for a resolved website.</summary>
public sealed class CaptchaChallengeDto
{
    /// <summary>"turnstile" or "math".</summary>
    public string Provider { get; set; } = "math";

    /// <summary>Turnstile public site key. Only set when Provider is "turnstile".</summary>
    public string? SiteKey { get; set; }

    /// <summary>Math-captcha token (hidden field) and inline SVG. Only set when Provider is "math".</summary>
    public string? Token { get; set; }
    public string? Svg { get; set; }
}
