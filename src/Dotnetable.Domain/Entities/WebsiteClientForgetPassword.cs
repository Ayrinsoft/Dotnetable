using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WebsiteClientForgetPassword
{
    public int WebsiteClientForgetPasswordID { get; set; }

    public string ForgetKey { get; set; } = null!;

    public int WebsiteClientID { get; set; }

    public DateTime LogTime { get; set; }

    /// <summary>Wrong-code attempts against this live code. The code dies once it passes the allowed limit.</summary>
    public int FailedAttempts { get; set; }

    /// <summary>Set when the attempt limit was passed; verify calls are refused until it elapses.</summary>
    public DateTime? LockedUntil { get; set; }

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;
}
