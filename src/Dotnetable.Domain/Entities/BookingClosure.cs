namespace Dotnetable.Domain.Entities;

/// <summary>A local date closed for booking. Null resource means the whole website.</summary>
public class BookingClosure
{
    public int BookingClosureID { get; set; }

    public int WebsiteID { get; set; }

    public int? BookingResourceID { get; set; }

    public DateOnly Date { get; set; }

    public string? Note { get; set; }

    public virtual BookingResource? Resource { get; set; }
}
