using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class MemberForgetPassword
{
    public int MemberForgetPasswordID { get; set; }

    public string ForgetKey { get; set; } = null!;

    public int MemberID { get; set; }

    public DateTime LogTime { get; set; }

    public virtual Member Member { get; set; } = null!;
}
