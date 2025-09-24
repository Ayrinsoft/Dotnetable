using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Policy
{
    public int PolicyID { get; set; }

    public string Title { get; set; }

    public bool Active { get; set; }

    public virtual ICollection<TBM_Policy_Role> TBM_Policy_Roles { get; set; } = new List<TBM_Policy_Role>();

    public virtual ICollection<TB_Member> TB_Members { get; set; } = new List<TB_Member>();
}
