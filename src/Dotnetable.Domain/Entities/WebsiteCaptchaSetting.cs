using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WebsiteCaptchaSetting
{
    public int WebsiteCaptchaSettingID { get; set; }

    public int WebsiteID { get; set; }

    public byte Provider { get; set; }

    public string? TurnstileSiteKey { get; set; }

    public string? TurnstileSecretKey { get; set; }

    public virtual Website Website { get; set; } = null!;
}
