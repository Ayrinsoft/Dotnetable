using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Email_Subscribe
{
    public int EmailSubscribeID { get; set; }

    public string Email { get; set; }

    public DateTime LogTime { get; set; }

    public bool Active { get; set; }

    public int? MemberID { get; set; }

    public bool Approved { get; set; }

    public virtual TB_Member Member { get; set; }
}
