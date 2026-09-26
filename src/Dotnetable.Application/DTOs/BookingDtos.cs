namespace Dotnetable.Application.DTOs;

public sealed class BookingCatalogDto
{
    public bool RequirePayment { get; set; }
    public bool AllowOnlinePayment { get; set; }
    public bool AllowOfflinePayment { get; set; }
    public string? BookThrough { get; set; }
    public string CurrencyCode { get; set; } = "";
    public List<BookingCatalogResourceDto> Resources { get; set; } = new();
}

public sealed class BookingCatalogResourceDto
{
    public int ResourceId { get; set; }
    public string Name { get; set; } = "";
    public string Hours { get; set; } = "";
    public List<BookingCatalogOfferingDto> Offerings { get; set; } = new();
}

public sealed class BookingCatalogOfferingDto
{
    public int OfferingId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal FullPrice { get; set; }
    public decimal DepositAmount { get; set; }
}

public sealed class BookingDayDto
{
    public string Date { get; set; } = "";
    public bool Open { get; set; }
}

public sealed class BookingSlotDto
{
    public string Start { get; set; } = "";
    public string End { get; set; } = "";
}

public sealed class BookingBookResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int? AppointmentId { get; set; }
    public int? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public bool NeedsPayment { get; set; }
}

public sealed class BookingDayRowDto
{
    public int AppointmentId { get; set; }
    public int ResourceId { get; set; }
    public string ResourceName { get; set; } = "";
    public string OfferingName { get; set; } = "";
    public string Start { get; set; } = "";
    public string End { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string? Phone { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = "";
    public decimal FullPrice { get; set; }
    public decimal DepositAmount { get; set; }
    public int? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public bool Paid { get; set; }
    public bool CanDelete { get; set; }
    public bool CanMove { get; set; }
}
