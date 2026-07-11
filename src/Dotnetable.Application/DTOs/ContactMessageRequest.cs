namespace Dotnetable.Application.DTOs;

/// <summary>Payload for a visitor-submitted contact form message.</summary>
public sealed class ContactMessageRequest
{
    public string SenderName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string CellphoneNumber { get; set; } = string.Empty;
    public string MessageSubject { get; set; } = string.Empty;
    public string MessageBody { get; set; } = string.Empty;

    /// <summary>Turnstile response token (when the site uses Turnstile) or the math-captcha token
    /// (hidden field paired with <see cref="CaptchaAnswer"/>).</summary>
    public string? CaptchaToken { get; set; }

    /// <summary>Answer to the math captcha; unused when the site uses Turnstile.</summary>
    public string? CaptchaAnswer { get; set; }

    /// <summary>Honeypot field: must stay empty. Hidden from real visitors with CSS; bots that
    /// autofill every field will trip it.</summary>
    public string? Website { get; set; }
}
