using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Policy
{
    public int PolicyID { get; set; }

    public string Title { get; set; } = null!;

    public bool Active { get; set; }

    public int WebsiteID { get; set; }

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual ICollection<PolicyRole> PolicyRoles { get; set; } = new List<PolicyRole>();

    public virtual Website Website { get; set; } = null!;
}
