using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class OrgUnit
{
    public int OrgUnitID { get; set; }
    public int WebsiteID { get; set; }
    public int? ParentOrgUnitID { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual Website Website { get; set; } = null!;
    public virtual OrgUnit? ParentOrgUnit { get; set; }
    public virtual ICollection<OrgUnit> InverseParentOrgUnit { get; set; } = new List<OrgUnit>();
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
