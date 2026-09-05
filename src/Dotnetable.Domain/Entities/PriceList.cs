using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// A published price list. Rows come from custom lines or from live catalog products.
/// Following the USD rate is optional (<see cref="Pricing"/>) — default is site currency.
/// </summary>
public partial class PriceList
{
    public int PriceListID { get; set; }

    public int WebsiteID { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public string? Notes { get; set; }

    /// <summary><see cref="Enums.PriceListSource"/>.</summary>
    public byte Source { get; set; }

    /// <summary><see cref="Enums.PriceListPricing"/>. Ignored when <see cref="Source"/> is catalog products.</summary>
    public byte Pricing { get; set; }

    /// <summary>Optional catalog filter when <see cref="Source"/> is catalog products. Null = every published product.</summary>
    public int? ProductCategoryID { get; set; }

    /// <summary>Optional brand filter when <see cref="Source"/> is catalog products.</summary>
    public int? BrandID { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual ProductCategory? ProductCategory { get; set; }

    public virtual ICollection<PriceListItem> PriceListItems { get; set; } = new List<PriceListItem>();

    public virtual Website Website { get; set; } = null!;
}
