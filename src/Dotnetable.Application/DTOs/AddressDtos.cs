namespace Dotnetable.Application.DTOs;

/// <summary>A country option for an address form dropdown.</summary>
public sealed class LocationOptionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

/// <summary>A customer address as returned by the API/website — flattened, no navigation cycles.</summary>
public sealed class AddressDto
{
    public int WebsiteClientAddressID { get; set; }
    public string? Title { get; set; }
    public string? ReceiverName { get; set; }
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public int? CityId { get; set; }
    public string? CityName { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>Payload for creating or updating a customer address.</summary>
public sealed class AddressRequest
{
    public string? Title { get; set; }
    public string? ReceiverName { get; set; }
    public int? CountryId { get; set; }
    public int? CityId { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public bool IsDefault { get; set; }
}
