namespace Dotnetable.Domain.Entities;

/// <summary>One booking setup per website: horizon, retention, payment rules, and the default weekly hours.</summary>
public class BookingProfile
{
    public int BookingProfileID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>IANA id when the host supports it, otherwise a Windows id such as Iran Standard Time.</summary>
    public string TimeZoneId { get; set; } = "Asia/Tehran";

    /// <summary>Last local date a customer may book, inclusive. Null until the admin sets it.</summary>
    public DateOnly? BookThrough { get; set; }

    /// <summary>Delete appointment rows this many days after they end. 0 keeps them.</summary>
    public int RetentionDays { get; set; } = 180;

    /// <summary>When false, a chosen slot is confirmed immediately and no invoice is created.</summary>
    public bool RequirePayment { get; set; } = true;

    public bool AllowOnlinePayment { get; set; } = true;

    public bool AllowOfflinePayment { get; set; } = true;

    /// <summary>How long an unpaid hold keeps the slot before it is released.</summary>
    public int HoldMinutes { get; set; } = 30;

    /// <summary>Customers cannot book a slot that starts sooner than this.</summary>
    public int LeadMinutes { get; set; } = 60;

    /// <summary>Default day start, minutes from local midnight. Copied onto new resources.</summary>
    public int DayStartMinutes { get; set; } = 9 * 60;

    public int DayEndMinutes { get; set; } = 17 * 60;

    /// <summary>Bitmask: Sat=1, Sun=2, Mon=4, Tue=8, Wed=16, Thu=32, Fri=64.</summary>
    public byte WorkDays { get; set; } = 31;

    public int? BreakStartMinutes { get; set; }

    public int? BreakEndMinutes { get; set; }

    public virtual Website Website { get; set; } = null!;
}
