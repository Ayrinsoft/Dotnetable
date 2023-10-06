using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TBM_Policy_Role
{
    public int PolicyRoleID { get; set; }

    public int PolicyID { get; set; }

    public short RoleID { get; set; }

    public bool Active { get; set; }

    public virtual TB_Policy Policy { get; set; }

    public virtual TB_Role Role { get; set; }
}
