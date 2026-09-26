namespace Dotnetable.Domain.Entities;

/// <summary>A reserved window. The linked order, when present, is the invoice for the deposit.</summary>
public class BookingAppointment
{
    public int BookingAppointmentID { get; set; }

    public int WebsiteID { get; set; }

    public int BookingResourceID { get; set; }

    public int BookingOfferingID { get; set; }

    public int? WebsiteClientID { get; set; }

    public int? OrderID { get; set; }

    public string CustomerName { get; set; } = "";

    public string? Phone { get; set; }

    public string? Note { get; set; }

    public DateTime StartsAtUtc { get; set; }

    public DateTime EndsAtUtc { get; set; }

    /// <summary><see cref="Enums.BookingAppointmentStatus"/>.</summary>
    public byte Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedByMemberID { get; set; }

    public virtual BookingResource Resource { get; set; } = null!;

    public virtual BookingOffering Offering { get; set; } = null!;

    public virtual Order? Order { get; set; }
}
