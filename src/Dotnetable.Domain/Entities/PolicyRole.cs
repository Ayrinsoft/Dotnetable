using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class PolicyRole
{
    public int PolicyRoleID { get; set; }

    public int PolicyID { get; set; }

    public short RoleID { get; set; }

    public bool Active { get; set; }

    public virtual Policy Policy { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
