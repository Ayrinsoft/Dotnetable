using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ShippingRate
{
    public int ShippingRateID { get; set; }

    public int ShippingMethodID { get; set; }

    public int? CountryID { get; set; }

    public int? StateID { get; set; }

    public int? CityID { get; set; }

    public decimal? MinWeightKg { get; set; }

    public decimal? MaxWeightKg { get; set; }

    public decimal PriceUsd { get; set; }

    public bool IsActive { get; set; }

    public virtual City? City { get; set; }

    public virtual Country? Country { get; set; }

    public virtual ShippingMethod ShippingMethod { get; set; } = null!;

    public virtual State? State { get; set; }
}
