using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Employee
{
    public int EmployeeID { get; set; }
    public int WebsiteID { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string GivenName { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string? NationalId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? OrgUnitID { get; set; }
    public string? JobTitle { get; set; }
    public int? MemberID { get; set; }
    /// <summary><see cref="Enums.EmployeeStatus"/>.</summary>
    public byte Status { get; set; } = 1;
    public DateOnly HireDate { get; set; }
    public DateOnly? TerminationDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
    public virtual OrgUnit? OrgUnit { get; set; }
    public virtual Member? Member { get; set; }
    public virtual ICollection<EmployeeContract> EmployeeContracts { get; set; } = new List<EmployeeContract>();
    public virtual ICollection<PayrollLine> PayrollLines { get; set; } = new List<PayrollLine>();
}
