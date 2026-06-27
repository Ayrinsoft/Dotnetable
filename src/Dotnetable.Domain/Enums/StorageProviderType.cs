namespace Dotnetable.Domain.Enums;

/// <summary>
/// Storage backends a website can register for file uploads.
/// Stored in <c>WebstieStorageSetting.StorageProvider</c> and <c>FileRecord.StorageProvider</c>.
/// </summary>
public enum StorageProviderType : short
{
    /// <summary>ArvanCloud object storage (S3-compatible).</summary>
    Arvan = 1,

    /// <summary>Dropbox.</summary>
    Dropbox = 2,

    /// <summary>Files stored on the API host's local disk, served via the API download endpoint.</summary>
    LocalHost = 3,

    /// <summary>Amazon Web Services S3 (S3-compatible).</summary>
    AwsS3 = 4,

    /// <summary>Cloudflare R2 (S3-compatible).</summary>
    CloudflareR2 = 5,

    /// <summary>Backblaze B2 via its S3-compatible endpoint.</summary>
    BackBlaze = 6,

    /// <summary>MinIO self-hosted object storage (S3-compatible).</summary>
    MinIO = 7,

    /// <summary>Azure Blob Storage.</summary>
    Azure = 8,

    /// <summary>Cloudinary media platform.</summary>
    Cloudinary = 9,

    /// <summary>Bunny.net Edge Storage + CDN pull zone.</summary>
    BunnyCDN = 10,
}
