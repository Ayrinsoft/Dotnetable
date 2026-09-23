using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

/// <summary>A pluggable storage backend (ArvanCloud, Dropbox, ...) for uploading/serving files.</summary>
public interface IFileStorageProvider
{
    StorageProviderType Provider { get; }

    /// <summary>
    /// Pushes one object to the backend and returns its storage path + public URL.
    /// <paramref name="cacheControl"/> is written as the object's Cache-Control header where the backend
    /// supports per-object headers; backends that do not simply ignore it.
    /// </summary>
    Task<StorageUploadResult> UploadAsync(StorageSettingContext ctx, Stream data, string storedName,
        string mimeType, string? cacheControl = null, CancellationToken ct = default);

    /// <summary>Removes a previously uploaded object. <paramref name="storedName"/> is the key/path used at upload.</summary>
    Task DeleteAsync(StorageSettingContext ctx, string storedName, CancellationToken ct = default);

    /// <summary>Live capacity snapshot. May return only <c>UsedKB</c> when the backend exposes no totals.</summary>
    Task<StorageQuota> GetQuotaAsync(StorageSettingContext ctx, CancellationToken ct = default);

    /// <summary>Verifies the configured credentials can reach the backend.</summary>
    Task<bool> TestConnectionAsync(StorageSettingContext ctx, CancellationToken ct = default);
}

/// <summary>
/// Optional capability for backends whose objects carry their own HTTP headers (S3-compatible stores).
/// Used by the background backfill to bring objects uploaded before the header existed up to date.
/// </summary>
public interface IStorageObjectMaintenance
{
    /// <summary>
    /// Rewrites the Cache-Control header of an existing object in place, keeping its Content-Type and
    /// public-read ACL. Returns false when the object does not exist.
    /// </summary>
    Task<bool> SetCacheControlAsync(StorageSettingContext ctx, string storedName, string cacheControl, CancellationToken ct = default);

    /// <summary>Creates a deny-all robots.txt at the bucket root unless one is already there.</summary>
    Task EnsureRobotsTxtAsync(StorageSettingContext ctx, CancellationToken ct = default);
}

/// <summary>Resolves the <see cref="IFileStorageProvider"/> for a given provider type.</summary>
public interface IFileStorageProviderRegistry
{
    IFileStorageProvider Get(StorageProviderType provider);
}
