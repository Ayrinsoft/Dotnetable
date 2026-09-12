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
    /// <summary>Virtual folder; null places the file under the library root.</summary>
    public int? FolderID { get; init; }
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

/// <summary>One place that references a media file (for delete confirmation).</summary>
public sealed class FileUsageItem
{
    /// <summary>Human-readable area, e.g. "Slideshow slides", "Product featured image".</summary>
    public required string Label { get; init; }

    public int Count { get; init; }

    /// <summary>Optional detail (names/titles) for the confirmation dialog.</summary>
    public string? Detail { get; init; }

    /// <summary>
    /// When true, delete will remove those rows (required FK). When false, the FK is only nulled.
    /// </summary>
    public bool IsRequired { get; init; }
}

/// <summary>Where a media file is currently used across the system.</summary>
public sealed class FileUsageSummary
{
    public IReadOnlyList<FileUsageItem> Items { get; init; } = Array.Empty<FileUsageItem>();

    public bool HasAny => Items.Count > 0;
    public bool HasRequired => Items.Any(i => i.IsRequired);
}

/// <summary>The media library: browsing, uploading, virtual folders and tags — all scoped per website.</summary>
public interface IFileService
{
    /// <summary>Paged file listing. <paramref name="websiteId"/> null = all websites (master only).</summary>
    Task<PagedResult<FileRecord>> GetPagedAsync(int? websiteId, FileFilter filter, GridQuery query, CancellationToken ct = default);

    Task<FileRecord?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Lists optional and required references for delete confirmation.</summary>
    Task<FileUsageSummary> GetUsageAsync(int id, CancellationToken ct = default);

    Task<FileRecord> UploadAsync(FileUploadRequest request, CancellationToken ct = default);

    /// <summary>
    /// Replaces a file's content while keeping its ID and storage key/URL — the storage object is
    /// overwritten in place, so every existing reference (an FK by ID or a URL already baked into
    /// published HTML) keeps working. Unlike <see cref="UploadAsync"/>, this never runs the
    /// raster→WebP conversion pass, since that can change the extension/key.
    /// </summary>
    Task<FileRecord> ReplaceContentAsync(int id, Stream content, string originalFileName, string? mimeType, CancellationToken ct = default);

    Task UpdateMetadataAsync(int id, string? title, string? altText, string? fileName, int? folderId, IReadOnlyList<int> tagIds, CancellationToken ct = default);

    /// <summary>
    /// Deletes the object (and thumbnail if any) from storage, nulls optional FKs that referenced
    /// this file, removes dependent slideshow slides that required it, and hard-deletes the
    /// <c>FileRecord</c> row.
    /// </summary>
    Task SoftDeleteAsync(int id, CancellationToken ct = default);

    // ── Virtual folders ──────────────────────────────────────
    Task<IReadOnlyList<FileFolder>> GetFoldersAsync(int websiteId, CancellationToken ct = default);
    Task<PagedResult<FileFolder>> GetFoldersPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default);
    Task<FileFolder> CreateFolderAsync(int websiteId, string name, string? description, int? parentFolderId = null, CancellationToken ct = default);
    Task UpdateFolderAsync(int folderId, string name, string? description, int? parentFolderId, CancellationToken ct = default);
    Task DeleteFolderAsync(int folderId, CancellationToken ct = default);

    // ── Tags ─────────────────────────────────────────────────
    Task<IReadOnlyList<FileTag>> GetTagsAsync(int websiteId, CancellationToken ct = default);
    Task<PagedResult<FileTag>> GetTagsPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default);
    Task<FileTag> CreateTagAsync(int websiteId, string name, CancellationToken ct = default);
    Task RenameTagAsync(int tagId, string name, CancellationToken ct = default);
    Task DeleteTagAsync(int tagId, CancellationToken ct = default);
}
