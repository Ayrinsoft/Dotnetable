using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Bot protection for the public admin forms. Decides between Cloudflare Turnstile and the
/// built-in math captcha, renders the math challenge, and verifies a submitted response.
/// </summary>
public interface IHumanVerificationService
{
    /// <summary>True when Turnstile is configured and selected (the widget should be rendered).</summary>
    bool TurnstileEnabled { get; }

    /// <summary>Public Turnstile site key for the widget, or null when Turnstile is not used.</summary>
    string? TurnstileSiteKey { get; }

    /// <summary>Creates a fresh math captcha and caches its answer for a few minutes.</summary>
    MathChallenge CreateMathChallenge();

    /// <summary>Validates a math captcha answer; consumes the challenge so it cannot be reused.</summary>
    bool ValidateMath(string? token, string? answer);

    /// <summary>Verifies a Turnstile response with Cloudflare. Returns null when Turnstile is unreachable.</summary>
    Task<bool?> VerifyTurnstileAsync(string? token, string? remoteIp, CancellationToken ct = default);

    /// <summary>Verifies a Turnstile response using a per-website secret key (public storefront forms)
    /// instead of the global admin secret. Returns null when Turnstile is unreachable.</summary>
    Task<bool?> VerifyTurnstileAsync(string? token, string? remoteIp, string secretKey, CancellationToken ct = default);

    /// <summary>
    /// Decides which captcha a website's public form should render: Turnstile when the site opted
    /// in and has both keys filled in, otherwise the built-in math captcha.
    /// </summary>
    CaptchaResolution ResolveForWebsite(WebsiteCaptchaSetting? setting);
}
