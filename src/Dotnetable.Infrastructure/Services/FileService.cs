using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Dotnetable.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly AppDbContext _context;
    private readonly IFileStorageProviderRegistry _providers;

    public FileService(AppDbContext context, IFileStorageProviderRegistry providers)
    {
        _context = context;
        _providers = providers;
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
        var setting = await _context.WebstieStorageSettings.AsNoTracking()
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

        buffer.Position = 0;
        var uploaded = await provider.UploadAsync(ctx, buffer, storedName, mime, ct);

        string? thumbStorage = null, thumbCdn = null;
        if (setting.AutoGenerateThumbnails && category == FileCategory.Image)
        {
            buffer.Position = 0;
            await using var thumb = await ImageThumbnailer.TryCreateAsync(buffer, ct);
            if (thumb is not null)
            {
                var thumbName = "t_" + storedName;
                var thumbResult = await provider.UploadAsync(ctx, thumb, thumbName, "image/jpeg", ct);
                thumbStorage = thumbResult.StoragePath;
                thumbCdn = thumbResult.CdnUrl;
            }
        }

        var record = new FileRecord
        {
            WebsiteStorageSettingsID = setting.WebsiteStorageSettingsID,
            StorageProvider = setting.StorageProvider,
            StoragePath = uploaded.StoragePath,
            CNDUrl = uploaded.CdnUrl,
            CDNFileCode = uploaded.CdnFileCode,
            OriginalFileName = Truncate(request.OriginalFileName, 120)!,
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

    private static StorageSettingContext ToContext(WebstieStorageSetting s) => new()
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
