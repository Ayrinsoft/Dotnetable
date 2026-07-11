using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Per-website bot-protection choice for public forms (contact form today). One row per website;
/// absence of a row (or an unfilled Turnstile key pair) means "fall back to the math captcha".
/// </summary>
public partial class WebsiteCaptchaSetting
{
    public int WebsiteCaptchaSettingID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>0 = Math, 1 = Turnstile. See <see cref="Enums.CaptchaProvider"/>.</summary>
    public byte Provider { get; set; }

    public string? TurnstileSiteKey { get; set; }

    public string? TurnstileSecretKey { get; set; }

    public virtual Website Website { get; set; } = null!;
}
