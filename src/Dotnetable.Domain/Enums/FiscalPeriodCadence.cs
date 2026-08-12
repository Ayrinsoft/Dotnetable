namespace Dotnetable.Domain.Enums;

/// <summary>How a website splits its accounting calendar (site owner setting).</summary>
public enum FiscalPeriodCadence : byte
{
    /// <summary>One period per calendar day.</summary>
    Daily = 1,
    /// <summary>One period per week (week start day on website settings).</summary>
    Weekly = 2,
    /// <summary>One period per month (default).</summary>
    Monthly = 3,
    /// <summary>One period per fiscal year.</summary>
    Yearly = 4,
}
