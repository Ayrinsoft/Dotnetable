using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class TaxRate
{
    public int TaxRateID { get; set; }

    public int WebsiteID { get; set; }

    public string Title { get; set; } = null!;

    public decimal Rate { get; set; }

    public int? CountryID { get; set; }

    public int? StateID { get; set; }

    public int Priority { get; set; }

    public bool IsActive { get; set; }

    public virtual Country? Country { get; set; }

    public virtual State? State { get; set; }

    public virtual Website Website { get; set; } = null!;
}
