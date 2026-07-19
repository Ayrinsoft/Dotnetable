using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using MudBlazor;

namespace Dotnetable.Admin.Components.Pages.Media;

/// <summary>Display helpers shared by the media-library pages/dialogs.</summary>
public static class MediaFormat
{
    /// <summary>Public CDN/download URL for embedding or sharing, if available.</summary>
    public static string? PublicUrl(FileRecord file)
    {
        if (!string.IsNullOrWhiteSpace(file.CNDUrl))
            return file.CNDUrl;
        if (!string.IsNullOrWhiteSpace(file.ThumbnailCDN))
            return file.ThumbnailCDN;
        return null;
    }

    /// <summary>Human-readable size from a KB value (e.g. 1536 → "1.5 MB").</summary>
    public static string Size(long kb)
    {
        double value = kb;
        string[] units = { "KB", "MB", "GB", "TB" };
        int i = 0;
        while (value >= 1024 && i < units.Length - 1) { value /= 1024; i++; }
        return $"{value:0.#} {units[i]}";
    }

    public static string CategoryIcon(byte category) => (FileCategory)category switch
    {
        FileCategory.Image => Icons.Material.Filled.Image,
        FileCategory.Document => Icons.Material.Filled.Description,
        FileCategory.Video => Icons.Material.Filled.Movie,
        FileCategory.Audio => Icons.Material.Filled.AudioFile,
        _ => Icons.Material.Filled.InsertDriveFile,
    };

    public static string ProviderLabel(StorageProviderType p) => p switch
    {
        StorageProviderType.Arvan => "ArvanCloud",
        StorageProviderType.Dropbox => "Dropbox",
        StorageProviderType.LocalHost => "Local (API host)",
        StorageProviderType.AwsS3 => "AWS S3",
        StorageProviderType.CloudflareR2 => "Cloudflare R2",
        StorageProviderType.BackBlaze => "Backblaze B2",
        StorageProviderType.MinIO => "MinIO",
        StorageProviderType.Azure => "Azure Blob",
        StorageProviderType.Cloudinary => "Cloudinary",
        StorageProviderType.BunnyCDN => "BunnyCDN",
        _ => p.ToString(),
    };

    /// <summary>Percentage 0-100 of quota used, or null when total is unknown.</summary>
    public static double? UsedPercent(StorageQuota? q) =>
        q is { TotalKB: long total and > 0 } ? Math.Min(100, q.UsedKB * 100d / total) : null;
}
