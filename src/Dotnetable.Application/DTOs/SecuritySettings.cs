namespace Dotnetable.Application.DTOs;

/// <summary>How the admin login/forgot-password forms protect themselves against bots.</summary>
public enum CaptchaMode
{
    /// <summary>Use Cloudflare Turnstile when its keys are configured, otherwise the math captcha.</summary>
    Auto = 0,

    /// <summary>Always use Cloudflare Turnstile (falls back to the math captcha if Turnstile is unreachable).</summary>
    Turnstile = 1,

    /// <summary>Always use the simple "add/subtract two numbers" image captcha.</summary>
    Math = 2,
}

/// <summary>
/// Anti-bot protection for the public admin forms. Persisted to the local settings file
/// (alongside the database connection) so it can be collected at setup and edited later.
/// </summary>
public class SecuritySettings
{
    /// <summary>Cloudflare Turnstile public site key, rendered in the widget. Empty = Turnstile disabled.</summary>
    public string TurnstileSiteKey { get; set; } = string.Empty;

    /// <summary>Cloudflare Turnstile secret key, used for server-side verification.</summary>
    public string TurnstileSecretKey { get; set; } = string.Empty;

    public CaptchaMode CaptchaMode { get; set; } = CaptchaMode.Auto;

    /// <summary>True when both Turnstile keys are present.</summary>
    public bool HasTurnstileKeys =>
        !string.IsNullOrWhiteSpace(TurnstileSiteKey) && !string.IsNullOrWhiteSpace(TurnstileSecretKey);

    /// <summary>Effective decision: should the Turnstile widget be shown (subject to runtime fallback)?</summary>
    public bool UseTurnstile => CaptchaMode switch
    {
        CaptchaMode.Math => false,
        CaptchaMode.Turnstile => HasTurnstileKeys,
        _ => HasTurnstileKeys, // Auto
    };
}
