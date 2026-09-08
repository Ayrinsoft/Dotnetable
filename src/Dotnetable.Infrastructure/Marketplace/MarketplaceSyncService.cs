using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Marketplace;

/// <summary>
/// Pushes the catalog to API-mode marketplace channels on their own schedule.
///
/// <para>Feed channels need nothing here — the engine crawls the published URL — so a channel only
/// takes part once an admin gives it a non-zero interval. Each pass takes only channels whose
/// interval has elapsed since <c>LastSyncAt</c>, so two hosts running this produce a re-push rather
/// than a corrupted state.</para>
/// </summary>
public sealed class MarketplaceSyncService : BackgroundService
{
    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDatabaseConfigStore _configStore;
    private readonly ILogger<MarketplaceSyncService> _logger;

    public MarketplaceSyncService(
        IServiceScopeFactory scopeFactory,
        IDatabaseConfigStore configStore,
        ILogger<MarketplaceSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _configStore = configStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Give the host a moment to finish starting; a first-run install has no database yet.
        await Task.Delay(TimeSpan.FromSeconds(45), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_configStore.IsConfigured)
                    await SweepAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Marketplace sync pass failed.");
            }

            try
            {
                await Task.Delay(SweepInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task SweepAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        var channelService = scope.ServiceProvider.GetRequiredService<IMarketplaceChannelService>();
        var registry = scope.ServiceProvider.GetRequiredService<IMarketplaceProviderRegistry>();

        List<(int Id, string Provider)> candidates;
        var now = DateTime.UtcNow;

        await using (var _context = await contextFactory.CreateDbContextAsync(ct))
        {
            candidates = await _context.MarketplaceChannels
                .AsNoTracking()
                .Where(c => c.IsActive && c.SyncIntervalMinutes > 0 &&
                    (c.LastSyncAt == null ||
                     c.LastSyncAt.Value.AddMinutes(c.SyncIntervalMinutes) <= now))
                .Select(c => ValueTuple.Create(c.MarketplaceChannelID, c.Provider))
                .ToListAsync(ct);
        }

        foreach (var candidate in candidates)
        {
            ct.ThrowIfCancellationRequested();

            // A feed channel is pulled by the engine; rebuilding it on a timer would only churn logs.
            if (registry.Find(candidate.Provider)?.Mode != MarketplaceIntegrationMode.Api) continue;

            try
            {
                var result = await channelService.SyncNowAsync(candidate.Id, "Schedule", ct);
                if (result.Status == MarketplaceSyncStatus.Failed)
                    _logger.LogWarning("Marketplace channel {ChannelId} sync failed: {Message}",
                        candidate.Id, result.Message);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Marketplace channel {ChannelId} sync threw.", candidate.Id);
            }
        }
    }
}
