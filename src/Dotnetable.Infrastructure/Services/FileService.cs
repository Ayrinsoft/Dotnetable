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
        if (filter.AlbumID is int albumId)
            q = q.Where(f => f.FileAlbumID == albumId);
        if (filter.TagID is int tagId)
            q = q.Where(f => f.FileRecordTags.Any(t => t.FileTagID == tagId));
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
        // are left untouched — every other raster format is converted to WebP for its smaller size.
        Stream uploadSource = buffer;
        var webpApplied = false;
        var isRasterProcessable = category == FileCategory.Image && ext is not (".svg" or ".gif");
        if (isRasterProcessable)
        {
            var options = await BuildProcessingOptionsAsync(request, ct);
            var convertToWebp = !string.Equals(mime, "image/webp", StringComparison.OrdinalIgnoreCase);
            if (options.HasWork || convertToWebp)
            {
                buffer.Position = 0;
                var format = convertToWebp ? SKEncodedImageFormat.Webp : MimeToFormat(mime);
                var processed = await ImageProcessor.TryProcessAsync(buffer, options, format, ct);
                if (processed is not null)
                {
                    uploadSource = processed;
                    sizeKb = (int)Math.Ceiling(processed.Length / 1024d);
                    if (convertToWebp)
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

    public async Task<FileTag> CreateTagAsync(int websiteId, string name, CancellationToken ct = default)
    {
        var tag = new FileTag { WebsiteID = websiteId, Name = Truncate(name, 60)! };
        _context.FileTags.Add(tag);
        await _context.SaveChangesAsync(ct);
        return tag;
    }

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

    private static SKEncodedImageFormat MimeToFormat(string mime) => mime.ToLowerInvariant() switch
    {
        "image/png" => SKEncodedImageFormat.Png,
        "image/webp" => SKEncodedImageFormat.Webp,
        "image/gif" => SKEncodedImageFormat.Gif,
        "image/bmp" => SKEncodedImageFormat.Bmp,
        _ => SKEncodedImageFormat.Jpeg,
    };

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
