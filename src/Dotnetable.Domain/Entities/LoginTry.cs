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

    public int WebsiteID { get; set; }
}
