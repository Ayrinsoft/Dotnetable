using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Filing / reporting window for sales VAT (distinct from accounting <see cref="FiscalPeriod"/>).
/// Snapshots are filled when the period is closed or when "Generate snapshot" is run.
/// </summary>
public partial class TaxPeriod
{
    public int TaxPeriodID { get; set; }
    public int WebsiteID { get; set; }
    /// <summary>Human code e.g. 2026-Q1 or 2026-03.</summary>
    public string PeriodCode { get; set; } = null!;
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    /// <summary><see cref="Enums.TaxPeriodStatus"/>.</summary>
    public byte Status { get; set; }
    public decimal OutputTaxSnapshot { get; set; }
    public decimal SettlementTaxSnapshot { get; set; }
    public decimal NetTaxSnapshot { get; set; }
    public int OutputOrderCount { get; set; }
    public int SettlementCount { get; set; }
    public string? Note { get; set; }
    public int? CreatedByMemberID { get; set; }
    public int? ClosedByMemberID { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? SnapshotAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
    public virtual Member? CreatedByMember { get; set; }
    public virtual Member? ClosedByMember { get; set; }
}
