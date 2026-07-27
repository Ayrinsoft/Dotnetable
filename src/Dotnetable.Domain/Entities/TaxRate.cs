using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class TaxRate
{
    public int TaxRateID { get; set; }

    public int WebsiteID { get; set; }

    public string Title { get; set; } = null!;

    /// <summary>Short code for invoices / export (e.g. VAT9, GST).</summary>
    public string? TaxCode { get; set; }

    /// <summary><see cref="Enums.TaxKind"/> — VAT, sales tax, or other.</summary>
    public byte TaxKind { get; set; }

    public decimal Rate { get; set; }

    public int? CountryID { get; set; }

    public int? StateID { get; set; }

    public int Priority { get; set; }

    /// <summary>When true, this rate also applies to shipping charges (if site TaxOnShipping is enabled).</summary>
    public bool ApplyToShipping { get; set; }

    public bool IsActive { get; set; }

    public virtual Country? Country { get; set; }

    public virtual State? State { get; set; }

    public virtual Website Website { get; set; } = null!;
}
