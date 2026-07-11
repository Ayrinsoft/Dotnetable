namespace Dotnetable.Domain.Enums;

/// <summary>Which bot-check a website's public forms (e.g. contact form) render.
/// See <see cref="Entities.WebsiteCaptchaSetting"/>.</summary>
public enum CaptchaProvider : byte
{
    Math = 0,
    Turnstile = 1,
}
