using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Normalized (0..1) crop rectangle applied to an image before resizing/storing.</summary>
public sealed class ImageCropRect
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
}

/// <summary>Request describing a single file to upload into the media library.</summary>
public sealed class FileUploadRequest
{
    public int WebsiteID { get; init; }
    public int StorageSettingID { get; init; }
    public required Stream Content { get; init; }
    public required string OriginalFileName { get; init; }
    public string? MimeType { get; init; }
    public int? AlbumID { get; init; }
    public IReadOnlyList<int>? TagIDs { get; init; }
    public string? Title { get; init; }
    public string? AltText { get; init; }

    /// <summary>Display name to store as <c>OriginalFileName</c> instead of the uploaded file's own name. The storage key is unaffected.</summary>
    public string? CustomFileName { get; init; }

    /// <summary>Normalized crop rectangle, applied before resize. Images only.</summary>
    public ImageCropRect? Crop { get; init; }

    /// <summary>Target width in pixels. When only one of Width/Height is set, aspect ratio is preserved.</summary>
    public int? ResizeWidth { get; init; }
    public int? ResizeHeight { get; init; }

    /// <summary>Converts the image to grayscale.</summary>
    public bool Grayscale { get; init; }

    /// <summary>Overrides the website's auto-apply watermark setting for this file. Null defers to the site setting.</summary>
    public bool? ApplyWatermark { get; init; }
}

/// <summary>The media library: browsing, uploading, albums and tags — all scoped per website.</summary>
public interface IFileService
{
    /// <summary>Paged file listing. <paramref name="websiteId"/> null = all websites (master only).</summary>
    Task<PagedResult<FileRecord>> GetPagedAsync(int? websiteId, FileFilter filter, GridQuery query, CancellationToken ct = default);

    Task<FileRecord?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<FileRecord> UploadAsync(FileUploadRequest request, CancellationToken ct = default);

    Task UpdateMetadataAsync(int id, string? title, string? altText, int? albumId, IReadOnlyList<int> tagIds, CancellationToken ct = default);

    /// <summary>Marks a file deleted (hidden from the library) without removing it from the backend.</summary>
    Task SoftDeleteAsync(int id, CancellationToken ct = default);

    // ── Albums ───────────────────────────────────────────────
    Task<IReadOnlyList<FileAlbum>> GetAlbumsAsync(int websiteId, CancellationToken ct = default);
    Task<FileAlbum> CreateAlbumAsync(int websiteId, string name, string? description, CancellationToken ct = default);
    Task RenameAlbumAsync(int albumId, string name, string? description, CancellationToken ct = default);
    Task DeleteAlbumAsync(int albumId, CancellationToken ct = default);

    // ── Tags ─────────────────────────────────────────────────
    Task<IReadOnlyList<FileTag>> GetTagsAsync(int websiteId, CancellationToken ct = default);
    Task<FileTag> CreateTagAsync(int websiteId, string name, CancellationToken ct = default);
    Task DeleteTagAsync(int tagId, CancellationToken ct = default);
}
