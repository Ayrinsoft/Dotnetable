using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

/// <summary>A pluggable storage backend (ArvanCloud, Dropbox, ...) for uploading/serving files.</summary>
public interface IFileStorageProvider
{
    StorageProviderType Provider { get; }

    /// <summary>Pushes one object to the backend and returns its storage path + public URL.</summary>
    Task<StorageUploadResult> UploadAsync(StorageSettingContext ctx, Stream data, string storedName,
        string mimeType, CancellationToken ct = default);

    /// <summary>Removes a previously uploaded object. <paramref name="storedName"/> is the key/path used at upload.</summary>
    Task DeleteAsync(StorageSettingContext ctx, string storedName, CancellationToken ct = default);

    /// <summary>Live capacity snapshot. May return only <c>UsedKB</c> when the backend exposes no totals.</summary>
    Task<StorageQuota> GetQuotaAsync(StorageSettingContext ctx, CancellationToken ct = default);

    /// <summary>Verifies the configured credentials can reach the backend.</summary>
    Task<bool> TestConnectionAsync(StorageSettingContext ctx, CancellationToken ct = default);
}

/// <summary>Resolves the <see cref="IFileStorageProvider"/> for a given provider type.</summary>
public interface IFileStorageProviderRegistry
{
    IFileStorageProvider Get(StorageProviderType provider);
}
