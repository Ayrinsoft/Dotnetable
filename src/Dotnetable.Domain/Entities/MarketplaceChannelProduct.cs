using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// A per-product override on top of the channel's category rules: an explicit include for a product
/// whose category is not offered, or an exclude for one that otherwise would be.
/// </summary>
public partial class MarketplaceChannelProduct
{
    public int MarketplaceChannelProductID { get; set; }

    public int MarketplaceChannelID { get; set; }

    public int ProductID { get; set; }

    /// <summary>True forces the product into the feed, false keeps it out regardless of its categories.</summary>
    public bool IsIncluded { get; set; }

    /// <summary>The channel's own identifier for this product, filled in after a successful API push.</summary>
    public string? RemoteProductID { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    public virtual MarketplaceChannel MarketplaceChannel { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
