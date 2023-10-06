using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Member_Forget_Password
{
    public int MemberForgetPasswordID { get; set; }

    public string ForgetKey { get; set; }

    public int MemberID { get; set; }

    public DateTime LogTime { get; set; }

    public virtual TB_Member Member { get; set; }
}
