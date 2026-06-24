using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Manages per-website storage backends (credentials, quota, activation).</summary>
public interface IStorageSettingService
{
    /// <summary>All storage settings for a website (raw rows, for the management grid).</summary>
    Task<IReadOnlyList<WebstieStorageSetting>> GetForWebsiteAsync(int websiteId, CancellationToken ct = default);

    Task<WebstieStorageSetting?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Active storages for a website with a live quota snapshot (for the upload selector).</summary>
    Task<IReadOnlyList<StorageSettingInfo>> GetActiveForWebsiteAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Quota snapshot for a single storage (live provider call + DB usage).</summary>
    Task<StorageQuota> GetQuotaAsync(int id, CancellationToken ct = default);

    Task<WebstieStorageSetting> CreateAsync(WebstieStorageSetting setting, CancellationToken ct = default);
    Task UpdateAsync(WebstieStorageSetting setting, CancellationToken ct = default);
    Task SetActiveAsync(int id, bool active, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Tests connectivity using the credentials of an existing setting.</summary>
    Task<bool> TestAsync(int id, CancellationToken ct = default);

    /// <summary>Tests connectivity for not-yet-saved credentials.</summary>
    Task<bool> TestAsync(StorageSettingContext ctx, CancellationToken ct = default);
}
