namespace Dotnetable.Application.DTOs;

/// <summary>Payload for a visitor newsletter signup.</summary>
public sealed class SubscribeRequest
{
    public string Email { get; set; } = string.Empty;

    /// <summary>Turnstile response token (when the site uses Turnstile) or the math-captcha token
    /// (hidden field paired with <see cref="CaptchaAnswer"/>).</summary>
    public string? CaptchaToken { get; set; }

    /// <summary>Answer to the math captcha; unused when the site uses Turnstile.</summary>
    public string? CaptchaAnswer { get; set; }

    /// <summary>Honeypot field: must stay empty.</summary>
    public string? Website { get; set; }
}
