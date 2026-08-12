namespace Dotnetable.Domain.Entities;

public partial class PayrollLine
{
    public int PayrollLineID { get; set; }
    public int PayrollRunID { get; set; }
    public int EmployeeID { get; set; }
    public decimal Gross { get; set; }
    public decimal EmployeeInsurance { get; set; }
    public decimal EmployerInsurance { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal Net { get; set; }
    /// <summary>Gross + employer insurance (total employer cost).</summary>
    public decimal EmployerCost { get; set; }
    public string? Note { get; set; }

    public virtual PayrollRun PayrollRun { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
}
