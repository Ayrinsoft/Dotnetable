namespace Dotnetable.Admin.Pages;

/// <summary>Read surface the captcha partials need from a page model.</summary>
public interface ICaptchaView
{
    /// <summary>True when the Turnstile widget should be the primary challenge (math stays as JS fallback).</summary>
    bool CaptchaUseTurnstile { get; }

    string? TurnstileSiteKey { get; }

    /// <summary>Inline SVG of the current math challenge.</summary>
    string CaptchaSvg { get; }

    /// <summary>Hidden-field token tying the submitted answer to the cached math challenge.</summary>
    string? MathToken { get; }

    /// <summary>Validation message to show under the captcha, when any.</summary>
    string? CaptchaError { get; }
}
