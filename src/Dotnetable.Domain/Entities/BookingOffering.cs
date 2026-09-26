namespace Dotnetable.Domain.Entities;

/// <summary>A service performed by one resource. Duration blocks that person's calendar.</summary>
public class BookingOffering
{
    public int BookingOfferingID { get; set; }

    public int WebsiteID { get; set; }

    public int BookingResourceID { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DurationMinutes { get; set; }

    /// <summary>Full service price in the website currency. Shown on the day book and the invoice title.</summary>
    public decimal FullPrice { get; set; }

    /// <summary>Amount charged to reserve. May be less than <see cref="FullPrice"/>.</summary>
    public decimal DepositAmount { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public virtual BookingResource Resource { get; set; } = null!;

    public virtual ICollection<BookingAppointment> Appointments { get; set; } = new List<BookingAppointment>();
}
