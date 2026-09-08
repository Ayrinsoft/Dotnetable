using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

/// <summary>Admin-side CRUD and operations over a website's registered marketplace channels.</summary>
public interface IMarketplaceChannelService
{
    Task<IReadOnlyList<MarketplaceChannelInfo>> GetForWebsiteAsync(int websiteId, CancellationToken ct = default);

    Task<MarketplaceChannelInfo?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(MarketplaceChannelInput input, CancellationToken ct = default);

    Task UpdateAsync(int id, MarketplaceChannelInput input, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>The channel's category rules, one row per category of the website.</summary>
    Task<IReadOnlyList<MarketplaceCategoryMapInfo>> GetCategoryMapAsync(int channelId, CancellationToken ct = default);

    /// <summary>Replaces the channel's category rules. Rows left at their defaults are dropped.</summary>
    Task SaveCategoryMapAsync(int channelId, IReadOnlyList<MarketplaceCategoryMapInfo> rows,
        CancellationToken ct = default);

    /// <summary>Per-product overrides: product ids explicitly forced in, and ids explicitly kept out.</summary>
    Task<(IReadOnlyList<int> Included, IReadOnlyList<int> Excluded)> GetProductOverridesAsync(int channelId,
        CancellationToken ct = default);

    Task SaveProductOverridesAsync(int channelId, IReadOnlyList<int> included, IReadOnlyList<int> excluded,
        CancellationToken ct = default);

    /// <summary>
    /// Runs the channel now: pushes to the API for API channels, or rebuilds and validates the
    /// document for feed channels so an admin can see the item count before handing the URL over.
    /// Writes a <c>MarketplaceSyncLog</c> row either way.
    /// </summary>
    Task<MarketplaceSyncResult> SyncNowAsync(int channelId, string triggeredBy = "Manual",
        CancellationToken ct = default);

    Task<IReadOnlyList<MarketplaceSyncLogInfo>> GetLogsAsync(int channelId, int take = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Builds the public feed for the channel owning <paramref name="feedToken"/>. Null when no active
    /// feed channel has that token.
    /// </summary>
    Task<MarketplaceFeedResult?> BuildFeedByTokenAsync(string feedToken, CancellationToken ct = default);
}

/// <summary>
/// Turns the catalog into the flat lines a channel is offered, applying that channel's category rules
/// and per-product overrides. Split out so providers and the feed endpoint share one definition of
/// "what this channel sells".
/// </summary>
public interface IMarketplaceProductSource
{
    Task<IReadOnlyList<MarketplaceProductItem>> GetItemsAsync(int channelId, CancellationToken ct = default);
}
