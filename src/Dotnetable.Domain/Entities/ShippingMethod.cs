using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ShippingMethod
{
    public int ShippingMethodID { get; set; }

    public int WebsiteID { get; set; }

    public string Title { get; set; } = null!;

    public string? CarrierName { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ShippingRate> ShippingRates { get; set; } = new List<ShippingRate>();

    public virtual Website Website { get; set; } = null!;
}
