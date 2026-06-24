using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

/// <summary>Strongly-typed view of a <c>WebstieStorageSetting</c> row used by the providers at runtime.</summary>
public sealed class StorageSettingContext
{
    public int WebsiteStorageSettingsID { get; init; }
    public int WebsiteID { get; init; }
    public StorageProviderType Provider { get; init; }

    /// <summary>Raw provider credentials/options JSON (parsed by each provider).</summary>
    public string SettingsJson { get; init; } = "{}";

    public long MaxFileSizeKB { get; init; }
    public string? AllowedExtensions { get; init; }
    public bool AutoGenerateThumbnails { get; init; }
}

/// <summary>Result of pushing a single object to a storage backend.</summary>
public sealed class StorageUploadResult
{
    /// <summary>Provider-internal path/key (e.g. S3 object key or Dropbox path).</summary>
    public string? StoragePath { get; init; }

    /// <summary>Publicly reachable CDN/download URL.</summary>
    public string? CdnUrl { get; init; }

    /// <summary>Provider file identifier, when one exists (e.g. Dropbox file id).</summary>
    public string? CdnFileCode { get; init; }
}

/// <summary>Capacity snapshot for a storage backend. <see cref="TotalKB"/> is null when unknown/unbounded.</summary>
public sealed class StorageQuota
{
    public long UsedKB { get; init; }
    public long? TotalKB { get; init; }
    public long? RemainingKB => TotalKB is long t ? Math.Max(0, t - UsedKB) : null;
}

/// <summary>ArvanCloud (S3-compatible) credentials, persisted in <c>StorageSettingsJSON</c>.</summary>
public sealed class ArvanStorageSettings
{
    public string Endpoint { get; set; } = "";
    public string BucketName { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";

    /// <summary>Public base URL for serving objects, e.g. https://my-bucket.s3.ir-thr-at1.arvanstorage.ir.</summary>
    public string PublicBaseUrl { get; set; } = "";

    /// <summary>Admin-declared plan capacity (S3 exposes no quota API). Null = unbounded.</summary>
    public long? CapacityKB { get; set; }
}

/// <summary>Dropbox credentials, persisted in <c>StorageSettingsJSON</c>.</summary>
public sealed class DropboxStorageSettings
{
    /// <summary>Short-lived access token (or a long-lived one for legacy apps).</summary>
    public string AccessToken { get; set; } = "";

    /// <summary>Optional refresh-token flow.</summary>
    public string? RefreshToken { get; set; }
    public string? AppKey { get; set; }
    public string? AppSecret { get; set; }

    /// <summary>Folder under which uploads are stored, e.g. /media. Defaults to app root.</summary>
    public string RootFolder { get; set; } = "";
}

/// <summary>A storage option shown in the admin UI / upload selector, with a live quota snapshot.</summary>
public sealed class StorageSettingInfo
{
    public int WebsiteStorageSettingsID { get; init; }
    public int WebsiteID { get; init; }
    public StorageProviderType Provider { get; init; }
    public string Label { get; init; } = "";
    public bool Active { get; init; }
    public long MaxFileSizeKB { get; init; }
    public string? AllowedExtensions { get; init; }
    public bool AutoGenerateThumbnails { get; init; }
    public StorageQuota? Quota { get; init; }
}
