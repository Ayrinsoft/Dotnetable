using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Login_Token
{
    public int LoginTokenID { get; set; }

    public int MemberID { get; set; }

    public Guid RefreshToken { get; set; }

    public DateTime ExpireTime { get; set; }

    public virtual TB_Member Member { get; set; }
}
