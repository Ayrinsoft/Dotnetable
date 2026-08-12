using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Progressive / stepped payroll statutory rates for a website (insurance &amp; income tax).
/// Applied when the employee contract does not force flat rates (or flat rates are zero and brackets exist).
/// </summary>
public partial class PayrollRateBracket
{
    public int PayrollRateBracketID { get; set; }
    public int WebsiteID { get; set; }
    /// <summary><see cref="Enums.PayrollRateKind"/>.</summary>
    public byte Kind { get; set; }
    /// <summary>Inclusive lower bound of the gross (or taxable) slice in site currency.</summary>
    public decimal FromAmount { get; set; }
    /// <summary>Exclusive upper bound; null = open-ended.</summary>
    public decimal? ToAmount { get; set; }
    /// <summary>Rate as fraction (e.g. 0.10 = 10%).</summary>
    public decimal Rate { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
}
