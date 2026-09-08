using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Per-channel treatment of one product category: whether its products are offered to the channel,
/// and which category id/path the channel expects for them. Every engine keeps its own taxonomy, so
/// the mapping has to be stored per channel rather than on <see cref="ProductCategory"/>.
/// </summary>
public partial class MarketplaceChannelCategory
{
    public int MarketplaceChannelCategoryID { get; set; }

    public int MarketplaceChannelID { get; set; }

    public int ProductCategoryID { get; set; }

    /// <summary>Only consulted when the channel is not set to include every product.</summary>
    public bool IsIncluded { get; set; } = true;

    /// <summary>The channel's own category identifier, when it requires one (Digikala, Google taxonomy id).</summary>
    public string? RemoteCategoryID { get; set; }

    /// <summary>The channel's category as a human-readable path, e.g. <c>Apparel &gt; Shoes</c>.</summary>
    public string? RemoteCategoryPath { get; set; }

    public virtual MarketplaceChannel MarketplaceChannel { get; set; } = null!;

    public virtual ProductCategory ProductCategory { get; set; } = null!;
}
