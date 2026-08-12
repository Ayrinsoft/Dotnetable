using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class PayrollRun
{
    public int PayrollRunID { get; set; }
    public int WebsiteID { get; set; }
    public string RunNumber { get; set; } = null!;
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    /// <summary><see cref="Enums.PayrollRunStatus"/>.</summary>
    public byte Status { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalEmployeeInsurance { get; set; }
    public decimal TotalEmployerInsurance { get; set; }
    public decimal TotalIncomeTax { get; set; }
    public decimal TotalNet { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public string? Note { get; set; }
    public int? CreatedByMemberID { get; set; }
    public int? ApprovedByMemberID { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
    public virtual Currency CurrencyCodeNavigation { get; set; } = null!;
    public virtual Member? CreatedByMember { get; set; }
    public virtual Member? ApprovedByMember { get; set; }
    public virtual ICollection<PayrollLine> PayrollLines { get; set; } = new List<PayrollLine>();
}
