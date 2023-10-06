using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TBM_Member_File
{
    public int MemberFileID { get; set; }

    public int MemberID { get; set; }

    public int FileID { get; set; }

    public virtual TB_File File { get; set; }

    public virtual TB_Member Member { get; set; }
}
