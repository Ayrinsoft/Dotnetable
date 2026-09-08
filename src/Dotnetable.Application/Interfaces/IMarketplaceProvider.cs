using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// One product search engine / marketplace integration. Registered in DI as one of many;
/// <see cref="IMarketplaceProviderRegistry"/> picks the one whose <see cref="Key"/> matches the
/// channel's configured provider — the same shape as <see cref="ISmsProvider"/>, so adding an engine
/// is a new class and a DI line, never a change to the product or order code.
/// </summary>
public interface IMarketplaceProvider
{
    /// <summary>Stable provider key stored in <c>MarketplaceChannel.Provider</c>, e.g. <c>GoogleShopping</c>.</summary>
    string Key { get; }

    /// <summary>Human-readable name for the admin channel picker.</summary>
    string DisplayName { get; }

    /// <summary>Whether the engine crawls a feed we publish or we push to its API.</summary>
    MarketplaceIntegrationMode Mode { get; }

    /// <summary>True when this engine is Iranian (used only for grouping in the UI).</summary>
    bool IsIranian { get; }

    /// <summary>False when the settings JSON is missing something the channel cannot run without.</summary>
    bool IsConfigured(MarketplaceChannelContext ctx);
}

/// <summary>
/// A provider whose catalog is served from a URL the engine crawls. Torob, Emalls and Google Shopping
/// all work this way: nothing is pushed, the document just has to be correct and reachable.
/// </summary>
public interface IMarketplaceFeedProvider : IMarketplaceProvider
{
    /// <summary>MIME type the feed is served under.</summary>
    string ContentType { get; }

    /// <summary>Serializes <paramref name="items"/> into the document this engine expects.</summary>
    MarketplaceFeedResult BuildFeed(MarketplaceChannelContext ctx, IReadOnlyList<MarketplaceProductItem> items);
}

/// <summary>
/// A provider that pushes the catalog to the engine's own API. Throws nothing on a remote error —
/// returns the failure so it can be written to the sync log.
/// </summary>
public interface IMarketplaceApiProvider : IMarketplaceProvider
{
    Task<MarketplaceSyncResult> PushAsync(MarketplaceChannelContext ctx,
        IReadOnlyList<MarketplaceProductItem> items, CancellationToken ct = default);
}

/// <summary>Resolves the <see cref="IMarketplaceProvider"/> registered for a provider key.</summary>
public interface IMarketplaceProviderRegistry
{
    /// <summary>The provider for <paramref name="key"/>, or null when none is registered.</summary>
    IMarketplaceProvider? Find(string? key);

    /// <summary>Every registered engine, for the admin picker.</summary>
    IReadOnlyList<IMarketplaceProvider> All { get; }
}
