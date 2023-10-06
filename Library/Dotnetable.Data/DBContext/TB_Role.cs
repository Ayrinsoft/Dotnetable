using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Role
{
    public short RoleID { get; set; }

    public string RoleKey { get; set; }

    public string Description { get; set; }

    public bool Active { get; set; }

    public virtual ICollection<TBM_Policy_Role> TBM_Policy_Roles { get; set; } = new List<TBM_Policy_Role>();
}
