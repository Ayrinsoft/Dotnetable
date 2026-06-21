using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class EmailSubscribe
{
    public int EmailSubscribeID { get; set; }

    public string Email { get; set; } = null!;

    public DateTime LogTime { get; set; }

    public bool Active { get; set; }

    public int? MemberID { get; set; }

    public bool Approved { get; set; }

    public virtual Member? Member { get; set; }
}
