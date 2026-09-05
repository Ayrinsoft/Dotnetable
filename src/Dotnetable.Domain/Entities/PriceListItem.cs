using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// One operator-authored row on a custom <see cref="PriceList"/>.
/// Default is a local <see cref="FixedPrice"/>. <see cref="LinkToUsd"/> is opt-in.
/// </summary>
public partial class PriceListItem
{
    public int PriceListItemID { get; set; }

    public int PriceListID { get; set; }

    public string Title { get; set; } = null!;

    public string? GroupName { get; set; }

    public string? Specification { get; set; }

    public string? Unit { get; set; }

    public string? Sku { get; set; }

    /// <summary>USD base used only when <see cref="LinkToUsd"/> is true.</summary>
    public decimal BasePriceUsd { get; set; }

    /// <summary>When true, storefront price is <see cref="BasePriceUsd"/> × current USD rate.</summary>
    public bool LinkToUsd { get; set; }

    /// <summary>Site-currency price used when not linked to USD.</summary>
    public decimal? FixedPrice { get; set; }

    public string? Notes { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual PriceList PriceList { get; set; } = null!;
}
