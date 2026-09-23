using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Brings files uploaded before the CDN-bandwidth work up to date, a small batch at a time:
/// builds the <c>{name}_thumb.webp</c> thumbnail for images that have none (or only the old
/// 320px <c>t_</c> one), stamps the immutable Cache-Control header onto existing S3 objects with a
/// server-side copy, and drops a deny-all robots.txt at each bucket root.
///
/// <para>Safe in more than one process: each row is claimed by bumping its attempt counter in the
/// same UPDATE that checks it, so two hosts never work the same file. A failed file stays pending
/// and is retried on later passes until <see cref="MaxAttempts"/>.</para>
/// </summary>
public sealed class FileMediaBackfillService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private const int ThumbnailBatch = 10;
    private const int CacheHeaderBatch = 50;
    private const byte MaxAttempts = 5;

    /// <summary>Originals larger than this are not downloaded for a thumbnail; they settle without one.</summary>
    private const long MaxSourceBytes = 40L * 1024 * 1024;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDatabaseConfigStore _configStore;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FileMediaBackfillService> _logger;

    /// <summary>Storage settings whose bucket robots.txt was already ensured by this process.</summary>
    private readonly HashSet<int> _robotsDone = new();

    public FileMediaBackfillService(
        IServiceScopeFactory scopeFactory,
        IDatabaseConfigStore configStore,
        IHttpClientFactory httpClientFactory,
        ILogger<FileMediaBackfillService> logger)
    {
        _scopeFactory = scopeFactory;
        _configStore = configStore;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_configStore.IsConfigured)
                    await RunAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File media backfill pass failed.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        var providers = scope.ServiceProvider.GetRequiredService<IFileStorageProviderRegistry>();

        // Providers that can rewrite an object's headers (the S3-compatible family).
        var maintainable = scope.ServiceProvider.GetServices<IFileStorageProvider>()
            .Where(p => p is IStorageObjectMaintenance)
            .Select(p => (short)p.Provider)
            .ToList();

        await EnsureRobotsAsync(contextFactory, providers, maintainable, ct);
        await BackfillThumbnailsAsync(contextFactory, providers, ct);
        await BackfillCacheHeadersAsync(contextFactory, providers, maintainable, ct);
    }

    // ── robots.txt ───────────────────────────────────────────
    private async Task EnsureRobotsAsync(IDbContextFactory<AppDbContext> contextFactory,
        IFileStorageProviderRegistry providers, List<short> maintainable, CancellationToken ct)
    {
        await using var _context = await contextFactory.CreateDbContextAsync(ct);

        var settings = await _context.WebsiteStorageSettings.AsNoTracking()
            .Where(s => s.Active && maintainable.Contains(s.StorageProvider))
            .ToListAsync(ct);

        foreach (var setting in settings.Where(s => !_robotsDone.Contains(s.WebsiteStorageSettingsID)))
        {
            try
            {
                if (providers.Get((StorageProviderType)setting.StorageProvider) is IStorageObjectMaintenance maintenance)
                    await maintenance.EnsureRobotsTxtAsync(ToContext(setting), ct);
                _robotsDone.Add(setting.WebsiteStorageSettingsID);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Could not ensure robots.txt for storage {StorageId}.", setting.WebsiteStorageSettingsID);
            }
        }
    }

    // ── Thumbnails ───────────────────────────────────────────
    private async Task BackfillThumbnailsAsync(IDbContextFactory<AppDbContext> contextFactory,
        IFileStorageProviderRegistry providers, CancellationToken ct)
    {
        await using var _context = await contextFactory.CreateDbContextAsync(ct);

        var pending = await _context.FileRecords.AsNoTracking()
            .Where(f => !f.IsDeleted
                && f.ThumbnailCheckedAt == null
                && f.ThumbnailAttempts < MaxAttempts
                && f.FileCategory == (byte)FileCategory.Image
                && f.WebsiteStorageSettings.Active
                && f.WebsiteStorageSettings.AutoGenerateThumbnails)
            .OrderBy(f => f.FileRecordID)
            .Select(f => new { f.FileRecordID, f.ThumbnailAttempts })
            .Take(ThumbnailBatch)
            .ToListAsync(ct);

        var done = 0;
        foreach (var item in pending)
        {
            if (!await ClaimAsync(_context, item.FileRecordID, item.ThumbnailAttempts, thumbnail: true, ct))
                continue;

            try
            {
                if (await BuildThumbnailAsync(_context, providers, item.FileRecordID, ct))
                    done++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Thumbnail backfill failed for file {FileId} (attempt {Attempt}).",
                    item.FileRecordID, item.ThumbnailAttempts + 1);
            }
        }

        if (done > 0)
            _logger.LogInformation("Built {Count} backfilled thumbnail(s).", done);
    }

    /// <summary>Makes (or settles without) one file's thumbnail. Returns true when a thumbnail was stored.</summary>
    private async Task<bool> BuildThumbnailAsync(AppDbContext _context, IFileStorageProviderRegistry providers,
        int fileId, CancellationToken ct)
    {
        var record = await _context.FileRecords
            .Include(f => f.WebsiteStorageSettings)
            .FirstAsync(f => f.FileRecordID == fileId, ct);

        var source = !ImageThumbnailer.Supports(record.StoredFileName)
            || !Uri.TryCreate(record.CNDUrl, UriKind.Absolute, out var uri)
            ? null
            : await DownloadAsync(uri, ct);

        await using var thumb = source is null ? null : await ImageThumbnailer.TryCreateAsync(source, ct);
        if (source is not null) await source.DisposeAsync();

        if (thumb is null)
        {
            // Not a thumbnail candidate, gone from the CDN, too large, or not decodable: settled, none.
            record.ThumbnailCheckedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return false;
        }

        var setting = record.WebsiteStorageSettings;
        var ctx = ToContext(setting);
        var provider = providers.Get((StorageProviderType)setting.StorageProvider);
        var result = await provider.UploadAsync(ctx, thumb, ImageThumbnailer.NameFor(record.StoredFileName),
            "image/webp", StorageCacheControl.Immutable, ct);

        var oldThumb = record.ThumbnailStorage;
        record.ThumbnailStorage = result.StoragePath;
        record.ThumbnailCDN = result.CdnUrl;
        record.ThumbnailCheckedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        // The old 320px "t_" thumbnail is replaced by the new key — remove it so nothing is orphaned.
        if (!string.IsNullOrWhiteSpace(oldThumb)
            && !string.Equals(oldThumb, result.StoragePath, StringComparison.Ordinal))
        {
            try
            {
                await provider.DeleteAsync(ctx, oldThumb, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Could not delete the old thumbnail '{Key}' of file {FileId}.", oldThumb, fileId);
            }
        }

        return true;
    }

    /// <summary>Downloads an original from its public URL; null when missing or over <see cref="MaxSourceBytes"/>.</summary>
    private async Task<MemoryStream?> DownloadAsync(Uri uri, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);
        if (response.StatusCode is System.Net.HttpStatusCode.NotFound or System.Net.HttpStatusCode.Gone)
            return null;
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength > MaxSourceBytes)
            return null;

        await using var body = await response.Content.ReadAsStreamAsync(ct);
        var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await body.ReadAsync(chunk, ct)) > 0)
        {
            if (buffer.Length + read > MaxSourceBytes)
            {
                await buffer.DisposeAsync();
                return null;
            }
            buffer.Write(chunk, 0, read);
        }
        buffer.Position = 0;
        return buffer;
    }

    // ── Cache-Control on existing objects ───────────────────
    private async Task BackfillCacheHeadersAsync(IDbContextFactory<AppDbContext> contextFactory,
        IFileStorageProviderRegistry providers, List<short> maintainable, CancellationToken ct)
    {
        if (maintainable.Count == 0) return;

        await using var _context = await contextFactory.CreateDbContextAsync(ct);

        var pending = await _context.FileRecords.AsNoTracking()
            .Where(f => !f.IsDeleted
                && f.CacheControlSetAt == null
                && f.CacheControlAttempts < MaxAttempts
                && maintainable.Contains(f.StorageProvider)
                && f.WebsiteStorageSettings.Active)
            .OrderBy(f => f.FileRecordID)
            .Select(f => new { f.FileRecordID, f.CacheControlAttempts })
            .Take(CacheHeaderBatch)
            .ToListAsync(ct);

        var done = 0;
        foreach (var item in pending)
        {
            if (!await ClaimAsync(_context, item.FileRecordID, item.CacheControlAttempts, thumbnail: false, ct))
                continue;

            try
            {
                var record = await _context.FileRecords
                    .Include(f => f.WebsiteStorageSettings)
                    .FirstAsync(f => f.FileRecordID == item.FileRecordID, ct);

                if (providers.Get((StorageProviderType)record.StorageProvider) is IStorageObjectMaintenance maintenance)
                {
                    var ctx = ToContext(record.WebsiteStorageSettings);
                    foreach (var key in new[] { record.StoragePath ?? record.StoredFileName, record.ThumbnailStorage })
                    {
                        // A missing object returns false; there is nothing to fix, so it still settles.
                        if (!string.IsNullOrWhiteSpace(key))
                            await maintenance.SetCacheControlAsync(ctx, key, StorageCacheControl.Immutable, ct);
                    }
                }

                record.CacheControlSetAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
                done++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Cache-Control backfill failed for file {FileId} (attempt {Attempt}).",
                    item.FileRecordID, item.CacheControlAttempts + 1);
            }
        }

        if (done > 0)
            _logger.LogInformation("Set Cache-Control on {Count} existing file(s).", done);
    }

    /// <summary>
    /// Atomically counts an attempt against one row and reports whether this process won it — the
    /// WHERE clause re-checks the attempt count we read, so a second host racing for it gets 0 rows.
    /// </summary>
    private static async Task<bool> ClaimAsync(AppDbContext _context, int fileId, byte seenAttempts, bool thumbnail,
        CancellationToken ct)
    {
        var next = (byte)(seenAttempts + 1);
        var rows = thumbnail
            ? await _context.FileRecords
                .Where(f => f.FileRecordID == fileId && f.ThumbnailCheckedAt == null && f.ThumbnailAttempts == seenAttempts)
                .ExecuteUpdateAsync(s => s.SetProperty(f => f.ThumbnailAttempts, next), ct)
            : await _context.FileRecords
                .Where(f => f.FileRecordID == fileId && f.CacheControlSetAt == null && f.CacheControlAttempts == seenAttempts)
                .ExecuteUpdateAsync(s => s.SetProperty(f => f.CacheControlAttempts, next), ct);
        return rows == 1;
    }

    private static StorageSettingContext ToContext(WebsiteStorageSetting s) => new()
    {
        WebsiteStorageSettingsID = s.WebsiteStorageSettingsID,
        WebsiteID = s.WebsiteID,
        Provider = (StorageProviderType)s.StorageProvider,
        SettingsJson = s.StorageSettingsJSON,
        MaxFileSizeKB = s.MaxFileSizeKB,
        AllowedExtensions = s.AllowedExtensions,
        AutoGenerateThumbnails = s.AutoGenerateThumbnails,
    };
}
