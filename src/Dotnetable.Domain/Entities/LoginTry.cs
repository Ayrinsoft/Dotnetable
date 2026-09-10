using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class LoginTry
{
    public int LoginTryID { get; set; }

    public string Username { get; set; } = null!;

    public DateTime LogTime { get; set; }

    public bool IsSuccess { get; set; }

    public string TryIP { get; set; } = null!;

    /// <summary>
    /// Null when the attempt cannot be attributed to a website (unknown username on the admin panel).
    /// Never 0 — there is no WebsiteID 0, and the FK would reject it.
    /// </summary>
    public int? WebsiteID { get; set; }

    public virtual Website? Website { get; set; }
}
