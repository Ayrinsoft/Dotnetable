using System;

namespace Dotnetable.Domain.Entities;

public partial class EmployeeContract
{
    public int EmployeeContractID { get; set; }
    public int EmployeeID { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    /// <summary>Gross base salary per pay period in site currency.</summary>
    public decimal BaseSalary { get; set; }
    public string CurrencyCode { get; set; } = null!;
    /// <summary>1=Monthly, 2=Biweekly, 3=Weekly.</summary>
    public byte PayFrequency { get; set; } = 1;
    /// <summary>Employee insurance rate as fraction (e.g. 0.07).</summary>
    public decimal EmployeeInsuranceRate { get; set; }
    /// <summary>Employer insurance rate as fraction.</summary>
    public decimal EmployerInsuranceRate { get; set; }
    /// <summary>Income tax rate as fraction (simplified flat rate for v1).</summary>
    public decimal IncomeTaxRate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
    public virtual Currency CurrencyCodeNavigation { get; set; } = null!;
}
