using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Member_Activate_Log
{
    public int MemberActivateLogID { get; set; }

    public int MemberID { get; set; }

    public Guid ActivateCode { get; set; }

    public DateTime ExpireDate { get; set; }

    public virtual TB_Member Member { get; set; }
}
