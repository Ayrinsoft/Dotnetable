using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Role
{
    public short RoleID { get; set; }

    public string RoleKey { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool Active { get; set; }

    /// <summary>Permission scope: 0 = admin panel, 1 = website customer (client). See RoleCategory.</summary>
    public byte Category { get; set; }

    public virtual ICollection<PolicyRole> PolicyRoles { get; set; } = new List<PolicyRole>();
}
