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
}
