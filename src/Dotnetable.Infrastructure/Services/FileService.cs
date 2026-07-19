using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Dotnetable.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace Dotnetable.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly AppDbContext _context;
    private readonly IFileStorageProviderRegistry _providers;
    private readonly IHttpClientFactory _httpClientFactory;

    public FileService(AppDbContext context, IFileStorageProviderRegistry providers, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _providers = providers;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<PagedResult<FileRecord>> GetPagedAsync(int? websiteId, FileFilter filter, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.FileRecords.AsNoTracking()
            .Include(f => f.FileAlbum)
            .Include(f => f.FileRecordTags).ThenInclude(t => t.FileTag)
            .Where(f => !f.IsDeleted);

        if (websiteId is int wid)
            q = q.Where(f => f.WebsiteStorageSettings.WebsiteID == wid);

        if (filter.Category is FileCategory cat)
            q = q.Where(f => f.FileCategory == (byte)cat);

        var albumIds = filter.EffectiveAlbumIDs().ToList();
        if (albumIds.Count == 1)
        {
            var albumId = albumIds[0];
            q = q.Where(f => f.FileAlbumID == albumId);
        }
        else if (albumIds.Count > 1)
        {
            q = q.Where(f => f.FileAlbumID != null && albumIds.Contains(f.FileAlbumID.Value));
        }

        var tagIds = filter.EffectiveTagIDs().ToList();
        if (tagIds.Count == 1)
        {
            var tagId = tagIds[0];
            q = q.Where(f => f.FileRecordTags.Any(t => t.FileTagID == tagId));
        }
        else if (tagIds.Count > 1)
        {
            q = q.Where(f => f.FileRecordTags.Any(t => tagIds.Contains(t.FileTagID)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();
            q = q.Where(f => f.OriginalFileName.Contains(s) || (f.Title != null && f.Title.Contains(s)));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(FileRecord.UploadDate), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<FileRecord> { Items = items, TotalCount = total };
    }

    public async Task<FileRecord?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.FileRecords
            .Include(f => f.FileAlbum)
            .Include(f => f.FileRecordTags).ThenInclude(t => t.FileTag)
            .FirstOrDefaultAsync(f => f.FileRecordID == id, ct);

    public async Task<FileRecord> UploadAsync(FileUploadRequest request, CancellationToken ct = default)
    {
        var setting = await _context.WebsiteStorageSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.WebsiteStorageSettingsID == request.StorageSettingID
                && s.WebsiteID == request.WebsiteID, ct)
            ?? throw new InvalidOperationException("Storage setting not found for this website.");
        if (!setting.Active)
            throw new InvalidOperationException("The selected storage is not active.");

        var ext = Path.GetExtension(request.OriginalFileName).ToLowerInvariant();
        ValidateExtension(setting.AllowedExtensions, ext);

        // Buffer so we can measure size and generate a thumbnail without a seekable source.
        await using var buffer = new MemoryStream();
        await request.Content.CopyToAsync(buffer, ct);
        var sizeKb = (int)Math.Ceiling(buffer.Length / 1024d);
        if (setting.MaxFileSizeKB > 0 && sizeKb > setting.MaxFileSizeKB)
            throw new InvalidOperationException($"File exceeds the {setting.MaxFileSizeKB} KB limit for this storage.");

        var mime = string.IsNullOrWhiteSpace(request.MimeType) ? "application/octet-stream" : request.MimeType!;
        var category = ClassifyMime(mime);
        var storedName = Guid.NewGuid().ToString("N") + ext;

        var ctx = ToContext(setting);
        var provider = _providers.Get((StorageProviderType)setting.StorageProvider);

        // SkiaSharp takes ownership of the decoded stream, so processing always reads from `buffer`
        // and yields a brand-new stream — `buffer` itself is never written to again afterwards.
        // SVG (vector) and GIF (SkiaSharp only decodes its first frame, which would kill animation)
        // are left untouched. Every other raster image is converted to lossy WebP for web delivery.
        // We pass the original byte length as a size budget so re-encode does not inflate a
        // well-compressed JPEG/PNG (e.g. 300 KB → multi‑MB lossless WebP).
        Stream uploadSource = buffer;
        var webpApplied = false;
        var isRasterProcessable = category == FileCategory.Image && ext is not (".svg" or ".gif");
        if (isRasterProcessable)
        {
            var options = await BuildProcessingOptionsAsync(request, ct);
            var alreadyWebp = string.Equals(mime, "image/webp", StringComparison.OrdinalIgnoreCase);
            // Re-encode when transforming, or when converting another format to WebP.
            // Already-WebP files with no transforms are stored as uploaded.
            if (options.HasWork || !alreadyWebp)
            {
                buffer.Position = 0;
                var originalLength = buffer.Length;
                var processed = await ImageProcessor.TryProcessAsync(
                    buffer, options, SKEncodedImageFormat.Webp, maxOutputBytes: originalLength, ct);
                if (processed is not null)
                {
                    uploadSource = processed;
                    sizeKb = (int)Math.Ceiling(processed.Length / 1024d);
                    if (!alreadyWebp)
                    {
                        mime = "image/webp";
                        ext = ".webp";
                        storedName = Path.GetFileNameWithoutExtension(storedName) + ext;
                        webpApplied = true;
                    }
                }
            }
        }

        uploadSource.Position = 0;
        var uploaded = await provider.UploadAsync(ctx, uploadSource, storedName, mime, ct);

        string? thumbStorage = null, thumbCdn = null;
        if (setting.AutoGenerateThumbnails && category == FileCategory.Image)
        {
            uploadSource.Position = 0;
            await using var thumb = await ImageThumbnailer.TryCreateAsync(uploadSource, ct);
            if (thumb is not null)
            {
                var thumbName = "t_" + storedName;
                var thumbResult = await provider.UploadAsync(ctx, thumb, thumbName, "image/jpeg", ct);
                thumbStorage = thumbResult.StoragePath;
                thumbCdn = thumbResult.CdnUrl;
            }
        }

        if (!ReferenceEquals(uploadSource, buffer))
            await uploadSource.DisposeAsync();

        var record = new FileRecord
        {
            WebsiteID = request.WebsiteID,
            WebsiteStorageSettingsID = setting.WebsiteStorageSettingsID,
            StorageProvider = setting.StorageProvider,
            StoragePath = uploaded.StoragePath,
            CNDUrl = uploaded.CdnUrl,
            CDNFileCode = uploaded.CdnFileCode,
            OriginalFileName = Truncate(webpApplied
                ? WithExtension(string.IsNullOrWhiteSpace(request.CustomFileName) ? request.OriginalFileName : request.CustomFileName, ext)
                : (string.IsNullOrWhiteSpace(request.CustomFileName) ? request.OriginalFileName : request.CustomFileName), 120)!,
            StoredFileName = storedName,
            MimeType = Truncate(mime, 74)!,
            FileSizeKB = sizeKb,
            FileCategory = (byte)category,
            Title = Truncate(request.Title, 50),
            AltText = Truncate(request.AltText, 120),
            FileAlbumID = request.AlbumID,
            ThumbnailStorage = thumbStorage,
            ThumbnailCDN = thumbCdn,
            IsDeleted = false,
            UploadDate = DateTime.UtcNow,
        };

        if (request.TagIDs is { Count: > 0 })
            foreach (var tagId in request.TagIDs.Distinct())
                record.FileRecordTags.Add(new FileRecordTag { FileTagID = tagId });

        _context.FileRecords.Add(record);
        await _context.SaveChangesAsync(ct);
        return record;
    }

    public async Task UpdateMetadataAsync(int id, string? title, string? altText, int? albumId,
        IReadOnlyList<int> tagIds, CancellationToken ct = default)
    {
        var record = await _context.FileRecords
            .Include(f => f.FileRecordTags)
            .FirstOrDefaultAsync(f => f.FileRecordID == id, ct)
            ?? throw new InvalidOperationException($"File {id} not found.");

        record.Title = Truncate(title, 50);
        record.AltText = Truncate(altText, 120);
        record.FileAlbumID = albumId;

        var desired = tagIds.Distinct().ToHashSet();
        foreach (var stale in record.FileRecordTags.Where(t => !desired.Contains(t.FileTagID)).ToList())
            record.FileRecordTags.Remove(stale);
        var existing = record.FileRecordTags.Select(t => t.FileTagID).ToHashSet();
        foreach (var tagId in desired.Where(t => !existing.Contains(t)))
            record.FileRecordTags.Add(new FileRecordTag { FileTagID = tagId });

        await _context.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken ct = default) =>
        await _context.FileRecords.Where(f => f.FileRecordID == id)
            .ExecuteUpdateAsync(s => s.SetProperty(f => f.IsDeleted, true), ct);

    // ── Albums ───────────────────────────────────────────────
    public async Task<IReadOnlyList<FileAlbum>> GetAlbumsAsync(int websiteId, CancellationToken ct = default) =>
        await _context.FileAlbums.AsNoTracking()
            .Where(a => a.WebsiteID == websiteId)
            .OrderBy(a => a.Name)
            .ToListAsync(ct);

    public async Task<PagedResult<FileAlbum>> GetAlbumsPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.FileAlbums.AsNoTracking()
            .Where(a => a.WebsiteID == websiteId);

        if (query.GetSearch(nameof(FileAlbum.Name)) is string name)
            q = q.Where(a => a.Name.Contains(name));
        if (query.GetSearch(nameof(FileAlbum.Description)) is string description)
            q = q.Where(a => a.Description != null && a.Description.Contains(description));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(FileAlbum.Name))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<FileAlbum> { Items = items, TotalCount = total };
    }

    public async Task<FileAlbum> CreateAlbumAsync(int websiteId, string name, string? description, CancellationToken ct = default)
    {
        var album = new FileAlbum
        {
            WebsiteID = websiteId,
            Name = Truncate(name, 120)!,
            Description = Truncate(description, 400),
            CreateDate = DateTime.UtcNow,
        };
        _context.FileAlbums.Add(album);
        await _context.SaveChangesAsync(ct);
        return album;
    }

    public async Task RenameAlbumAsync(int albumId, string name, string? description, CancellationToken ct = default) =>
        await _context.FileAlbums.Where(a => a.FileAlbumID == albumId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.Name, Truncate(name, 120)!)
                .SetProperty(a => a.Description, Truncate(description, 400)), ct);

    public async Task DeleteAlbumAsync(int albumId, CancellationToken ct = default)
    {
        // Detach files from the album, then remove it.
        await _context.FileRecords.Where(f => f.FileAlbumID == albumId)
            .ExecuteUpdateAsync(s => s.SetProperty(f => f.FileAlbumID, (int?)null), ct);
        await _context.FileAlbums.Where(a => a.FileAlbumID == albumId).ExecuteDeleteAsync(ct);
    }

    // ── Tags ─────────────────────────────────────────────────
    public async Task<IReadOnlyList<FileTag>> GetTagsAsync(int websiteId, CancellationToken ct = default) =>
        await _context.FileTags.AsNoTracking()
            .Where(t => t.WebsiteID == websiteId)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

    public async Task<PagedResult<FileTag>> GetTagsPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.FileTags.AsNoTracking()
            .Where(t => t.WebsiteID == websiteId);

        if (query.GetSearch(nameof(FileTag.Name)) is string name)
            q = q.Where(t => t.Name.Contains(name));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(FileTag.Name))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<FileTag> { Items = items, TotalCount = total };
    }

    public async Task<FileTag> CreateTagAsync(int websiteId, string name, CancellationToken ct = default)
    {
        var tag = new FileTag { WebsiteID = websiteId, Name = Truncate(name, 60)! };
        _context.FileTags.Add(tag);
        await _context.SaveChangesAsync(ct);
        return tag;
    }

    public async Task RenameTagAsync(int tagId, string name, CancellationToken ct = default) =>
        await _context.FileTags.Where(t => t.FileTagID == tagId)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.Name, Truncate(name, 60)!), ct);

    public async Task DeleteTagAsync(int tagId, CancellationToken ct = default)
    {
        await _context.FileRecordTags.Where(t => t.FileTagID == tagId).ExecuteDeleteAsync(ct);
        await _context.FileTags.Where(t => t.FileTagID == tagId).ExecuteDeleteAsync(ct);
    }

    // ── Image processing ────────────────────────────────────────
    private async Task<ImageProcessingOptions> BuildProcessingOptionsAsync(FileUploadRequest request, CancellationToken ct)
    {
        WatermarkOptions? watermark = null;

        var setting = await _context.WebsiteWatermarkSettings.AsNoTracking()
            .Include(s => s.WatermarkFile)
            .FirstOrDefaultAsync(s => s.WebsiteID == request.WebsiteID, ct);

        var shouldApply = setting is { WatermarkFile.CNDUrl: not null }
            && (request.ApplyWatermark ?? setting.Enabled);

        if (shouldApply)
        {
            var bytes = await TryDownloadAsync(setting!.WatermarkFile!.CNDUrl!, ct);
            if (bytes is not null)
            {
                watermark = new WatermarkOptions
                {
                    ImageBytes = bytes,
                    Position = (WatermarkPosition)setting.Position,
                    SizePercent = setting.SizePercent,
                    Opacity = setting.Opacity,
                };
            }
        }

        return new ImageProcessingOptions
        {
            Crop = request.Crop,
            ResizeWidth = request.ResizeWidth,
            ResizeHeight = request.ResizeHeight,
            Grayscale = request.Grayscale,
            Watermark = watermark,
        };
    }

    private async Task<byte[]?> TryDownloadAsync(string url, CancellationToken ct)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return null;
        try
        {
            var client = _httpClientFactory.CreateClient();
            return await client.GetByteArrayAsync(uri, ct);
        }
        catch
        {
            return null;
        }
    }

    // ── Helpers ──────────────────────────────────────────────
    private static FileCategory ClassifyMime(string mime)
    {
        mime = mime.ToLowerInvariant();
        if (mime.StartsWith("image/")) return FileCategory.Image;
        if (mime.StartsWith("video/")) return FileCategory.Video;
        if (mime.StartsWith("audio/")) return FileCategory.Audio;
        if (mime.StartsWith("application/pdf") || mime.StartsWith("text/")
            || mime.Contains("word") || mime.Contains("excel") || mime.Contains("spreadsheet")
            || mime.Contains("presentation") || mime.Contains("officedocument"))
            return FileCategory.Document;
        return FileCategory.Other;
    }

    private static void ValidateExtension(string? allowed, string ext)
    {
        if (string.IsNullOrWhiteSpace(allowed)) return;
        var normalized = ext.TrimStart('.');
        var ok = allowed.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().TrimStart('.').ToLowerInvariant())
            .Contains(normalized);
        if (!ok)
            throw new InvalidOperationException($"File type '{ext}' is not allowed for this storage.");
    }

    private static string? Truncate(string? value, int max) =>
        string.IsNullOrEmpty(value) ? value : (value.Length <= max ? value : value[..max]);

    private static string WithExtension(string fileName, string ext)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        return string.IsNullOrEmpty(name) ? "file" + ext : name + ext;
    }

    private static StorageSettingContext ToContext(WebsiteStorageSetting s) => new()
    {
        WebsiteStorageSettingsID = s.WebsiteStorageSettingsID,
        WebsiteID = s.WebsiteID,
        Provider = (StorageProviderType)s.StorageProvider,
        SettingsJson = s.StorageSettingsJSON,
        MaxFileSizeKB = s.MaxFileSizeKB,
        AllowedExtensions = s.AllowedExtensions,
        AutoGenerateThumbnails = s.AutoGenerateThumbnails,
    };
}
