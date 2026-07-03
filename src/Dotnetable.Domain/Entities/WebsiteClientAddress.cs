using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WebsiteClientAddress
{
    public int WebsiteClientAddressID { get; set; }

    public int WebsiteClientID { get; set; }

    public string? Title { get; set; }

    public string? ReceiverName { get; set; }

    public int? CountryId { get; set; }

    public int? CityId { get; set; }

    public string AddressLine { get; set; } = null!;

    public string? PostalCode { get; set; }

    public string? Phone { get; set; }

    public bool IsDefault { get; set; }

    public virtual City? City { get; set; }

    public virtual Country? Country { get; set; }

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;
}
