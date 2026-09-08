using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// A per-website registration of a product search engine / marketplace (Torob, Emalls, Google
/// Shopping, Digikala…), mirroring <see cref="WebsiteSmsSetting"/>: the provider key selects an
/// <c>IMarketplaceProvider</c> implementation and <see cref="SettingsJSON"/> carries that provider's
/// own credentials and field mapping.
/// </summary>
public partial class MarketplaceChannel
{
    public int MarketplaceChannelID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Provider key, e.g. <c>GoogleShopping</c>, <c>GenericFeed</c>. Matches <c>IMarketplaceProvider.Key</c>.</summary>
    public string Provider { get; set; } = null!;

    /// <summary>Label shown in the admin list.</summary>
    public string Title { get; set; } = null!;

    /// <summary>Provider credentials/options JSON, parsed by the matching provider.</summary>
    public string SettingsJSON { get; set; } = "{}";

    /// <summary>
    /// Random slug in the public feed URL (<c>/feeds/{FeedToken}</c>) for feed providers. Unguessable
    /// so the catalog is not enumerable by website id.
    /// </summary>
    public string FeedToken { get; set; } = null!;

    /// <summary>
    /// When true every active product is offered to the channel and <see cref="MarketplaceChannelCategories"/>
    /// only carries the remote category mapping. When false a product must be in an included category.
    /// </summary>
    public bool IncludeAllProducts { get; set; } = true;

    public bool IsActive { get; set; }

    public int SortOrder { get; set; }

    /// <summary>Minutes between automatic pushes for API channels. Zero disables the schedule.</summary>
    public int SyncIntervalMinutes { get; set; }

    public DateTime? LastSyncAt { get; set; }

    /// <summary>Outcome of the last run, mirroring <c>MarketplaceSyncStatus</c>.</summary>
    public byte? LastSyncStatus { get; set; }

    public string? LastSyncMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual ICollection<MarketplaceChannelCategory> MarketplaceChannelCategories { get; set; } = new List<MarketplaceChannelCategory>();

    public virtual ICollection<MarketplaceChannelProduct> MarketplaceChannelProducts { get; set; } = new List<MarketplaceChannelProduct>();

    public virtual ICollection<MarketplaceSyncLog> MarketplaceSyncLogs { get; set; } = new List<MarketplaceSyncLog>();
}
