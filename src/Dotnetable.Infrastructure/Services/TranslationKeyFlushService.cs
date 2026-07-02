using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Periodically drains <see cref="PendingTranslationKeys"/> and inserts any localization keys that
/// pages requested but the database didn't have yet. Runs in every host that calls
/// <c>AddInfrastructure</c>, so newly-used UI strings self-register after the first run.
/// </summary>
public sealed class TranslationKeyFlushService : BackgroundService
{
    private const int MaxKeyLength = 72;     // mirrors LocalizationKey.ItemKey
    private const int MaxValueLength = 2000; // mirrors LocalizationKey.DefaultValue
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

    private readonly PendingTranslationKeys _pending;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TranslationKeyFlushService> _logger;

    public TranslationKeyFlushService(
        PendingTranslationKeys pending,
        IServiceScopeFactory scopeFactory,
        ILogger<TranslationKeyFlushService> logger)
    {
        _pending = pending;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await FlushAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // Never let a transient DB issue stop the loop; retry next tick.
                _logger.LogWarning(ex, "Flushing new localization keys failed; will retry.");
            }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (TaskCanceledException) { break; }
        }
    }

    private async Task FlushAsync(CancellationToken ct)
    {
        if (_pending.IsEmpty) return;

        var batch = _pending.Drain();
        if (batch.Count == 0) return;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await factory.CreateDbContextAsync(ct);

        foreach (var byWebsite in batch.GroupBy(b => b.WebsiteId))
        {
            var websiteId = byWebsite.Key;
            var candidates = byWebsite
                .Where(b => b.Key.Length <= MaxKeyLength)
                .GroupBy(b => b.Key)
                .Select(g => g.First())
                .ToList();
            if (candidates.Count == 0) continue;

            var keys = candidates.Select(c => c.Key).ToList();
            var existing = await db.LocalizationKeys
                .Where(k => k.WebsiteID == websiteId && keys.Contains(k.ItemKey))
                .Select(k => k.ItemKey)
                .ToListAsync(ct);
            var existingSet = existing.ToHashSet();

            foreach (var (_, key, defaultValue) in candidates)
            {
                if (!existingSet.Add(key)) continue; // already present
                db.LocalizationKeys.Add(new LocalizationKey
                {
                    WebsiteID = websiteId,
                    ItemKey = key,
                    DefaultValue = Truncate(defaultValue, MaxValueLength),
                });
            }
        }

        var inserted = await db.SaveChangesAsync(ct);
        if (inserted > 0)
            _logger.LogInformation("Registered {Count} new localization key(s).", inserted);
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}
