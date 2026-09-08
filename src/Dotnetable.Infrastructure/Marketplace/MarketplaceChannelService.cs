using System.Security.Cryptography;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Marketplace;

/// <summary>Resolves the <see cref="IMarketplaceProvider"/> registered for a provider key.</summary>
public sealed class MarketplaceProviderRegistry : IMarketplaceProviderRegistry
{
    private readonly IReadOnlyList<IMarketplaceProvider> _providers;

    public MarketplaceProviderRegistry(IEnumerable<IMarketplaceProvider> providers) =>
        _providers = providers.ToList();

    public IReadOnlyList<IMarketplaceProvider> All => _providers;

    public IMarketplaceProvider? Find(string? key) => string.IsNullOrWhiteSpace(key)
        ? null
        : _providers.FirstOrDefault(p => string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase));
}

/// <summary>Admin-side CRUD and operations over a website's marketplace channels.</summary>
public sealed class MarketplaceChannelService : IMarketplaceChannelService
{
    private const int LogRetentionCount = 100;

    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IMarketplaceProviderRegistry _registry;
    private readonly IMarketplaceProductSource _productSource;

    public MarketplaceChannelService(IDbContextFactory<AppDbContext> contextFactory,
        IMarketplaceProviderRegistry registry, IMarketplaceProductSource productSource)
    {
        _contextFactory = contextFactory;
        _registry = registry;
        _productSource = productSource;
    }

    public async Task<IReadOnlyList<MarketplaceChannelInfo>> GetForWebsiteAsync(int websiteId,
        CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var rows = await _context.MarketplaceChannels
            .AsNoTracking()
            .Where(c => c.WebsiteID == websiteId)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.MarketplaceChannelID)
            .ToListAsync(ct);

        var baseUrl = await SiteBaseUrlAsync(_context, websiteId, ct);
        return rows.Select(r => ToInfo(r, baseUrl)).ToList();
    }

    public async Task<MarketplaceChannelInfo?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var row = await _context.MarketplaceChannels
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.MarketplaceChannelID == id, ct);
        if (row is null) return null;

        return ToInfo(row, await SiteBaseUrlAsync(_context, row.WebsiteID, ct));
    }

    public async Task<int> CreateAsync(MarketplaceChannelInput input, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var row = new MarketplaceChannel
        {
            WebsiteID = input.WebsiteID,
            Provider = input.Provider.Trim(),
            Title = input.Title.Trim(),
            SettingsJSON = string.IsNullOrWhiteSpace(input.SettingsJSON) ? "{}" : input.SettingsJSON,
            FeedToken = NewFeedToken(),
            IncludeAllProducts = input.IncludeAllProducts,
            IsActive = input.IsActive,
            SortOrder = input.SortOrder,
            SyncIntervalMinutes = input.SyncIntervalMinutes,
            CreatedAt = DateTime.UtcNow,
        };

        _context.MarketplaceChannels.Add(row);
        await _context.SaveChangesAsync(ct);
        return row.MarketplaceChannelID;
    }

    public async Task UpdateAsync(int id, MarketplaceChannelInput input, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var row = await _context.MarketplaceChannels.FirstOrDefaultAsync(c => c.MarketplaceChannelID == id, ct);
        if (row is null) return;

        row.Provider = input.Provider.Trim();
        row.Title = input.Title.Trim();
        row.SettingsJSON = string.IsNullOrWhiteSpace(input.SettingsJSON) ? "{}" : input.SettingsJSON;
        row.IncludeAllProducts = input.IncludeAllProducts;
        row.IsActive = input.IsActive;
        row.SortOrder = input.SortOrder;
        row.SyncIntervalMinutes = input.SyncIntervalMinutes;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var row = await _context.MarketplaceChannels.FirstOrDefaultAsync(c => c.MarketplaceChannelID == id, ct);
        if (row is null) return;

        _context.MarketplaceChannels.Remove(row);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<MarketplaceCategoryMapInfo>> GetCategoryMapAsync(int channelId,
        CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var channel = await _context.MarketplaceChannels
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.MarketplaceChannelID == channelId, ct);
        if (channel is null) return [];

        var categories = await _context.ProductCategories
            .AsNoTracking()
            .Where(c => c.WebsiteID == channel.WebsiteID && c.IsActive)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new { c.ProductCategoryID, c.Name })
            .ToListAsync(ct);

        var rules = await _context.MarketplaceChannelCategories
            .AsNoTracking()
            .Where(r => r.MarketplaceChannelID == channelId)
            .ToDictionaryAsync(r => r.ProductCategoryID, ct);

        return categories.Select(c =>
        {
            rules.TryGetValue(c.ProductCategoryID, out var rule);
            return new MarketplaceCategoryMapInfo
            {
                ProductCategoryID = c.ProductCategoryID,
                CategoryName = c.Name,
                IsIncluded = rule?.IsIncluded ?? false,
                RemoteCategoryID = rule?.RemoteCategoryID,
                RemoteCategoryPath = rule?.RemoteCategoryPath,
            };
        }).ToList();
    }

    public async Task SaveCategoryMapAsync(int channelId, IReadOnlyList<MarketplaceCategoryMapInfo> rows,
        CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.MarketplaceChannelCategories
            .Where(r => r.MarketplaceChannelID == channelId)
            .ToDictionaryAsync(r => r.ProductCategoryID, ct);

        foreach (var row in rows)
        {
            var carriesNothing = !row.IsIncluded &&
                string.IsNullOrWhiteSpace(row.RemoteCategoryID) &&
                string.IsNullOrWhiteSpace(row.RemoteCategoryPath);

            if (existing.TryGetValue(row.ProductCategoryID, out var current))
            {
                if (carriesNothing)
                {
                    _context.MarketplaceChannelCategories.Remove(current);
                    continue;
                }

                current.IsIncluded = row.IsIncluded;
                current.RemoteCategoryID = Blank(row.RemoteCategoryID);
                current.RemoteCategoryPath = Blank(row.RemoteCategoryPath);
            }
            else if (!carriesNothing)
            {
                _context.MarketplaceChannelCategories.Add(new MarketplaceChannelCategory
                {
                    MarketplaceChannelID = channelId,
                    ProductCategoryID = row.ProductCategoryID,
                    IsIncluded = row.IsIncluded,
                    RemoteCategoryID = Blank(row.RemoteCategoryID),
                    RemoteCategoryPath = Blank(row.RemoteCategoryPath),
                });
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<(IReadOnlyList<int> Included, IReadOnlyList<int> Excluded)> GetProductOverridesAsync(
        int channelId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var rows = await _context.MarketplaceChannelProducts
            .AsNoTracking()
            .Where(p => p.MarketplaceChannelID == channelId)
            .Select(p => new { p.ProductID, p.IsIncluded })
            .ToListAsync(ct);

        return (rows.Where(r => r.IsIncluded).Select(r => r.ProductID).ToList(),
                rows.Where(r => !r.IsIncluded).Select(r => r.ProductID).ToList());
    }

    public async Task SaveProductOverridesAsync(int channelId, IReadOnlyList<int> included,
        IReadOnlyList<int> excluded, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.MarketplaceChannelProducts
            .Where(p => p.MarketplaceChannelID == channelId)
            .ToListAsync(ct);

        var wanted = included.Select(id => (ProductId: id, IsIncluded: true))
            .Concat(excluded.Select(id => (ProductId: id, IsIncluded: false)))
            .GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.First().IsIncluded);

        foreach (var row in existing)
        {
            if (wanted.TryGetValue(row.ProductID, out var isIncluded))
            {
                row.IsIncluded = isIncluded;
                wanted.Remove(row.ProductID);
            }
            else
            {
                _context.MarketplaceChannelProducts.Remove(row);
            }
        }

        foreach (var (productId, isIncluded) in wanted)
        {
            _context.MarketplaceChannelProducts.Add(new MarketplaceChannelProduct
            {
                MarketplaceChannelID = channelId,
                ProductID = productId,
                IsIncluded = isIncluded,
            });
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<MarketplaceSyncResult> SyncNowAsync(int channelId, string triggeredBy = "Manual",
        CancellationToken ct = default)
    {
        MarketplaceChannelContext context;
        int logId;

        await using (var _context = await _contextFactory.CreateDbContextAsync(ct))
        {
            var channel = await _context.MarketplaceChannels
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.MarketplaceChannelID == channelId, ct);
            if (channel is null) return MarketplaceSyncResult.Fail("This channel no longer exists.");

            context = await ToContextAsync(_context, channel, ct);

            var log = new MarketplaceSyncLog
            {
                MarketplaceChannelID = channelId,
                StartedAt = DateTime.UtcNow,
                Status = (byte)MarketplaceSyncStatus.Running,
                TriggeredBy = triggeredBy,
            };
            _context.MarketplaceSyncLogs.Add(log);
            await _context.SaveChangesAsync(ct);
            logId = log.MarketplaceSyncLogID;
        }

        var result = await RunAsync(context, ct);
        await RecordAsync(channelId, logId, result, ct);
        return result;
    }

    public async Task<IReadOnlyList<MarketplaceSyncLogInfo>> GetLogsAsync(int channelId, int take = 20,
        CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.MarketplaceSyncLogs
            .AsNoTracking()
            .Where(l => l.MarketplaceChannelID == channelId)
            .OrderByDescending(l => l.MarketplaceSyncLogID)
            .Take(take < 1 ? 1 : take)
            .Select(l => new MarketplaceSyncLogInfo
            {
                MarketplaceSyncLogID = l.MarketplaceSyncLogID,
                StartedAt = l.StartedAt,
                FinishedAt = l.FinishedAt,
                Status = (MarketplaceSyncStatus)l.Status,
                TriggeredBy = l.TriggeredBy,
                ItemCount = l.ItemCount,
                FailedCount = l.FailedCount,
                Message = l.Message,
            })
            .ToListAsync(ct);
    }

    public async Task<MarketplaceFeedResult?> BuildFeedByTokenAsync(string feedToken,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(feedToken)) return null;

        MarketplaceChannelContext context;
        int channelId;

        await using (var _context = await _contextFactory.CreateDbContextAsync(ct))
        {
            var channel = await _context.MarketplaceChannels
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.FeedToken == feedToken && c.IsActive, ct);
            if (channel is null) return null;

            channelId = channel.MarketplaceChannelID;
            context = await ToContextAsync(_context, channel, ct);
        }

        if (_registry.Find(context.Provider) is not IMarketplaceFeedProvider provider) return null;

        var items = await _productSource.GetItemsAsync(channelId, ct);
        var feed = provider.BuildFeed(context, items);

        await MarkFeedServedAsync(channelId, feed.ItemCount, ct);
        return feed;
    }

    /// <summary>
    /// A feed channel has nothing to push, so "sync" builds the document and reports what it would
    /// serve — enough for an admin to confirm the mapping before handing the URL to the engine.
    /// </summary>
    private async Task<MarketplaceSyncResult> RunAsync(MarketplaceChannelContext context, CancellationToken ct)
    {
        var provider = _registry.Find(context.Provider);
        if (provider is null)
            return MarketplaceSyncResult.Fail($"No provider is registered for '{context.Provider}'.");
        if (!provider.IsConfigured(context))
            return MarketplaceSyncResult.Fail("This channel's settings are incomplete.");

        var items = await _productSource.GetItemsAsync(context.MarketplaceChannelID, ct);

        try
        {
            return provider switch
            {
                IMarketplaceApiProvider api => await api.PushAsync(context, items, ct),
                IMarketplaceFeedProvider feed => Built(feed.BuildFeed(context, items)),
                _ => MarketplaceSyncResult.Fail("This provider supports neither a feed nor an API push."),
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return MarketplaceSyncResult.Fail(ex.Message);
        }

        static MarketplaceSyncResult Built(MarketplaceFeedResult feed) =>
            MarketplaceSyncResult.Ok(feed.ItemCount, $"Feed rebuilt with {feed.ItemCount} items.");
    }

    private async Task RecordAsync(int channelId, int logId, MarketplaceSyncResult result, CancellationToken ct)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var log = await _context.MarketplaceSyncLogs.FirstOrDefaultAsync(l => l.MarketplaceSyncLogID == logId, ct);
        if (log is not null)
        {
            log.FinishedAt = DateTime.UtcNow;
            log.Status = (byte)result.Status;
            log.ItemCount = result.ItemCount;
            log.FailedCount = result.FailedCount;
            log.Message = Truncate(result.Message, 1000);
        }

        var channel = await _context.MarketplaceChannels.FirstOrDefaultAsync(c => c.MarketplaceChannelID == channelId, ct);
        if (channel is not null)
        {
            channel.LastSyncAt = DateTime.UtcNow;
            channel.LastSyncStatus = (byte)result.Status;
            channel.LastSyncMessage = Truncate(result.Message, 1000);
        }

        await _context.SaveChangesAsync(ct);
        await TrimLogsAsync(_context, channelId, ct);
    }

    /// <summary>A crawled feed writes its own log row so the admin can see the engine is actually fetching.</summary>
    private async Task MarkFeedServedAsync(int channelId, int itemCount, CancellationToken ct)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var now = DateTime.UtcNow;
        _context.MarketplaceSyncLogs.Add(new MarketplaceSyncLog
        {
            MarketplaceChannelID = channelId,
            StartedAt = now,
            FinishedAt = now,
            Status = (byte)MarketplaceSyncStatus.Success,
            TriggeredBy = "Feed",
            ItemCount = itemCount,
            Message = $"Feed served with {itemCount} items.",
        });

        var channel = await _context.MarketplaceChannels.FirstOrDefaultAsync(c => c.MarketplaceChannelID == channelId, ct);
        if (channel is not null)
        {
            channel.LastSyncAt = now;
            channel.LastSyncStatus = (byte)MarketplaceSyncStatus.Success;
            channel.LastSyncMessage = $"Feed served with {itemCount} items.";
        }

        await _context.SaveChangesAsync(ct);
        await TrimLogsAsync(_context, channelId, ct);
    }

    /// <summary>A crawled feed can be fetched many times a day; only the recent history is worth keeping.</summary>
    private static async Task TrimLogsAsync(AppDbContext _context, int channelId, CancellationToken ct)
    {
        var cutoff = await _context.MarketplaceSyncLogs
            .Where(l => l.MarketplaceChannelID == channelId)
            .OrderByDescending(l => l.MarketplaceSyncLogID)
            .Skip(LogRetentionCount)
            .Select(l => l.MarketplaceSyncLogID)
            .FirstOrDefaultAsync(ct);
        if (cutoff == 0) return;

        await _context.MarketplaceSyncLogs
            .Where(l => l.MarketplaceChannelID == channelId && l.MarketplaceSyncLogID <= cutoff)
            .ExecuteDeleteAsync(ct);
    }

    private static async Task<MarketplaceChannelContext> ToContextAsync(AppDbContext _context,
        MarketplaceChannel channel, CancellationToken ct)
    {
        var website = await _context.Websites
            .AsNoTracking()
            .Where(w => w.WebsiteID == channel.WebsiteID)
            .Select(w => new { w.WebsiteAddress, w.DefaultCurrencyCode, w.BrandName })
            .FirstOrDefaultAsync(ct);

        return new MarketplaceChannelContext
        {
            MarketplaceChannelID = channel.MarketplaceChannelID,
            WebsiteID = channel.WebsiteID,
            Provider = channel.Provider,
            Title = string.IsNullOrWhiteSpace(channel.Title) ? website?.BrandName ?? "" : channel.Title,
            SettingsJson = channel.SettingsJSON,
            SiteBaseUrl = Absolute(website?.WebsiteAddress),
            CurrencyCode = website?.DefaultCurrencyCode ?? "IRR",
        };
    }

    private MarketplaceChannelInfo ToInfo(MarketplaceChannel row, string baseUrl)
    {
        var provider = _registry.Find(row.Provider);
        var context = new MarketplaceChannelContext
        {
            MarketplaceChannelID = row.MarketplaceChannelID,
            WebsiteID = row.WebsiteID,
            Provider = row.Provider,
            Title = row.Title,
            SettingsJson = row.SettingsJSON,
        };

        return new MarketplaceChannelInfo
        {
            MarketplaceChannelID = row.MarketplaceChannelID,
            WebsiteID = row.WebsiteID,
            Provider = row.Provider,
            ProviderDisplayName = provider?.DisplayName ?? row.Provider,
            Mode = provider?.Mode ?? MarketplaceIntegrationMode.Feed,
            Title = row.Title,
            SettingsJSON = row.SettingsJSON,
            FeedToken = row.FeedToken,
            IncludeAllProducts = row.IncludeAllProducts,
            IsActive = row.IsActive,
            SortOrder = row.SortOrder,
            SyncIntervalMinutes = row.SyncIntervalMinutes,
            LastSyncAt = row.LastSyncAt,
            LastSyncStatus = row.LastSyncStatus is null ? null : (MarketplaceSyncStatus)row.LastSyncStatus.Value,
            LastSyncMessage = row.LastSyncMessage,
            IsConfigured = provider?.IsConfigured(context) ?? false,
            FeedUrl = provider?.Mode == MarketplaceIntegrationMode.Feed && baseUrl.Length > 0
                ? $"{baseUrl}/feeds/{row.FeedToken}"
                : null,
            CreatedAt = row.CreatedAt,
        };
    }

    private static async Task<string> SiteBaseUrlAsync(AppDbContext _context, int websiteId, CancellationToken ct)
    {
        var address = await _context.Websites
            .AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .Select(w => w.WebsiteAddress)
            .FirstOrDefaultAsync(ct);
        return Absolute(address);
    }

    private static string Absolute(string? websiteAddress)
    {
        var address = (websiteAddress ?? "").Trim().TrimEnd('/');
        if (address.Length == 0) return "";
        return address.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               address.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? address
            : $"https://{address}";
    }

    /// <summary>URL-safe, unguessable, and short enough to paste into a seller panel.</summary>
    private static string NewFeedToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Truncate(string? value, int max) =>
        value is null || value.Length <= max ? value : value[..max];
}
