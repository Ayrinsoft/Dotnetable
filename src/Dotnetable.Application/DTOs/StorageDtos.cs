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

/// <summary>Local on-host disk storage, persisted in <c>StorageSettingsJSON</c>. Files are served by the API download endpoint.</summary>
public sealed class LocalStorageSettings
{
    /// <summary>Directory that holds uploaded files. Absolute, or relative to the API host's content root. Defaults to <c>App_Data/uploads</c>.</summary>
    public string BasePath { get; set; } = "App_Data/uploads";

    /// <summary>Public base URL of the API download endpoint, e.g. https://api.mysite.com/api/files (no trailing website id).</summary>
    public string PublicBaseUrl { get; set; } = "";

    /// <summary>Admin-declared capacity for the quota bar. Null = unbounded.</summary>
    public long? CapacityKB { get; set; }
}

/// <summary>Generic S3-compatible credentials shared by AWS S3, Cloudflare R2, Backblaze B2 and MinIO. Persisted in <c>StorageSettingsJSON</c>.</summary>
public sealed class S3StorageSettings
{
    /// <summary>Custom service endpoint (required for R2 / Backblaze / MinIO). Leave blank for AWS S3 and use <see cref="Region"/> instead.</summary>
    public string Endpoint { get; set; } = "";

    /// <summary>AWS region system name (e.g. eu-central-1). Used when <see cref="Endpoint"/> is blank.</summary>
    public string Region { get; set; } = "";

    public string BucketName { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";

    /// <summary>Public base URL for serving objects, e.g. https://my-bucket.r2.dev.</summary>
    public string PublicBaseUrl { get; set; } = "";

    /// <summary>Path-style addressing. Required by R2 / Backblaze / MinIO; AWS S3 typically uses virtual-hosted style (false).</summary>
    public bool ForcePathStyle { get; set; } = true;

    /// <summary>Admin-declared plan capacity (S3 exposes no quota API). Null = unbounded.</summary>
    public long? CapacityKB { get; set; }
}

/// <summary>Azure Blob Storage credentials, persisted in <c>StorageSettingsJSON</c>.</summary>
public sealed class AzureStorageSettings
{
    public string ConnectionString { get; set; } = "";
    public string ContainerName { get; set; } = "";

    /// <summary>Optional public/CDN base URL. When blank the blob's own URL is used.</summary>
    public string PublicBaseUrl { get; set; } = "";

    /// <summary>Admin-declared capacity for the quota bar. Null = unbounded.</summary>
    public long? CapacityKB { get; set; }
}

/// <summary>Cloudinary credentials, persisted in <c>StorageSettingsJSON</c>.</summary>
public sealed class CloudinaryStorageSettings
{
    public string CloudName { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ApiSecret { get; set; } = "";

    /// <summary>Optional folder uploads are placed under.</summary>
    public string Folder { get; set; } = "";

    /// <summary>Admin-declared capacity for the quota bar. Null = unbounded.</summary>
    public long? CapacityKB { get; set; }
}

/// <summary>Bunny.net Edge Storage + CDN credentials, persisted in <c>StorageSettingsJSON</c>.</summary>
public sealed class BunnyStorageSettings
{
    public string StorageZoneName { get; set; } = "";

    /// <summary>Storage-zone password used as the <c>AccessKey</c> header.</summary>
    public string AccessKey { get; set; } = "";

    /// <summary>Storage region code (e.g. ny, la, sg, de). Blank = main (Falkenstein) region.</summary>
    public string Region { get; set; } = "";

    /// <summary>Public pull-zone URL for serving files, e.g. https://my-zone.b-cdn.net.</summary>
    public string PullZoneUrl { get; set; } = "";

    /// <summary>Admin-declared capacity for the quota bar. Null = unbounded.</summary>
    public long? CapacityKB { get; set; }
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
