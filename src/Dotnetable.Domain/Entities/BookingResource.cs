namespace Dotnetable.Domain.Entities;

/// <summary>One bookable person or chair. Appointments on the same resource cannot overlap.</summary>
public class BookingResource
{
    public int BookingResourceID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public int DayStartMinutes { get; set; } = 9 * 60;

    public int DayEndMinutes { get; set; } = 17 * 60;

    /// <summary>Bitmask: Sat=1, Sun=2, Mon=4, Tue=8, Wed=16, Thu=32, Fri=64.</summary>
    public byte WorkDays { get; set; } = 31;

    public int? BreakStartMinutes { get; set; }

    public int? BreakEndMinutes { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual ICollection<BookingOffering> Offerings { get; set; } = new List<BookingOffering>();

    public virtual ICollection<BookingAppointment> Appointments { get; set; } = new List<BookingAppointment>();
}
