using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class CurrencyRate
{
    public int CurrencyRateID { get; set; }

    public int WebsiteID { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public decimal USDToCurrency { get; set; }

    public bool IsDefault { get; set; }

    public DateTime LastUpdate { get; set; }

    public virtual Website Website { get; set; } = null!;
}
