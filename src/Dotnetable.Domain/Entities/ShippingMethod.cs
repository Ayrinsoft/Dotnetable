using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ShippingMethod
{
    public int ShippingMethodID { get; set; }

    public int WebsiteID { get; set; }

    public string Title { get; set; } = null!;

    /// <summary>Carrier brand/name shown at checkout (e.g. Tipax, Post, Chapar).</summary>
    public string? CarrierName { get; set; }

    public int? LogoFileID { get; set; }

    /// <summary>When true, prepaid shipping is offered at checkout.</summary>
    public bool SupportsPrepaid { get; set; } = true;

    /// <summary>When true, cash-on-delivery / postpay shipping is offered at checkout.</summary>
    public bool SupportsCod { get; set; } = true;

    /// <summary>Minimum prepaid shipping price in site operational currency (floor over zone rates).</summary>
    public decimal PrepaidMinPrice { get; set; }

    /// <summary>USD dual / conversion bridge for <see cref="PrepaidMinPrice"/>.</summary>
    public decimal PrepaidMinPriceUsd { get; set; }

    /// <summary>Minimum COD/postpay shipping price in site operational currency.</summary>
    public decimal CodMinPrice { get; set; }

    /// <summary>USD dual / conversion bridge for <see cref="CodMinPrice"/>.</summary>
    public decimal CodMinPriceUsd { get; set; }

    /// <summary>
    /// Free-shipping threshold for this method in site currency. When cart subtotal is greater than or equal to this
    /// amount (and the amount is &gt; 0), prepaid/COD shipping prices resolve to zero. Zero means no free-shipping floor.
    /// </summary>
    public decimal FreeShippingMinOrderAmount { get; set; }

    /// <summary>USD dual / conversion bridge for <see cref="FreeShippingMinOrderAmount"/>.</summary>
    public decimal FreeShippingMinOrderAmountUsd { get; set; }

    public bool IsActive { get; set; }

    public int SortOrder { get; set; }

    public virtual FileRecord? LogoFile { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ShippingRate> ShippingRates { get; set; } = new List<ShippingRate>();

    public virtual Website Website { get; set; } = null!;
}
