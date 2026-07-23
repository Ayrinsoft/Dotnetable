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
            .Include(f => f.FileFolder)
            .Include(f => f.FileRecordTags).ThenInclude(t => t.FileTag)
            .Where(f => !f.IsDeleted);

        if (websiteId is int wid)
            q = q.Where(f => f.WebsiteStorageSettings.WebsiteID == wid);

        if (filter.Category is FileCategory cat)
            q = q.Where(f => f.FileCategory == (byte)cat);

        switch (filter.FolderScope)
        {
            case FileFolderScope.Root:
                q = q.Where(f => f.FileFolderID == null);
                break;
            case FileFolderScope.Folder when filter.FolderID is int folderId:
                q = q.Where(f => f.FileFolderID == folderId);
                break;
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
            .Include(f => f.FileFolder)
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
                    // Graphics-heavy PNGs (QR codes, icons) often re-encode larger as WebP.
                    // Keep the original bytes whenever we only converted format and grew the file.
                    // When the user applied crop/resize/etc., always keep the processed result.
                    if (!options.HasWork && processed.Length >= originalLength)
                    {
                        await processed.DisposeAsync();
                    }
                    else
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
                var thumbName = "t_" + Path.GetFileNameWithoutExtension(storedName) + ".webp";
                var thumbResult = await provider.UploadAsync(ctx, thumb, thumbName, "image/webp", ct);
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
            FileFolderID = request.FolderID,
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

    public async Task UpdateMetadataAsync(int id, string? title, string? altText, int? folderId,
        IReadOnlyList<int> tagIds, CancellationToken ct = default)
    {
        var record = await _context.FileRecords
            .Include(f => f.FileRecordTags)
            .FirstOrDefaultAsync(f => f.FileRecordID == id, ct)
            ?? throw new InvalidOperationException($"File {id} not found.");

        record.Title = Truncate(title, 50);
        record.AltText = Truncate(altText, 120);
        record.FileFolderID = folderId;

        var desired = tagIds.Distinct().ToHashSet();
        foreach (var stale in record.FileRecordTags.Where(t => !desired.Contains(t.FileTagID)).ToList())
            record.FileRecordTags.Remove(stale);
        var existing = record.FileRecordTags.Select(t => t.FileTagID).ToHashSet();
        foreach (var tagId in desired.Where(t => !existing.Contains(t)))
            record.FileRecordTags.Add(new FileRecordTag { FileTagID = tagId });

        await _context.SaveChangesAsync(ct);
    }

    public async Task<FileUsageSummary> GetUsageAsync(int id, CancellationToken ct = default)
    {
        var items = new List<FileUsageItem>();

        async Task AddOptionalAsync(string label, Task<int> countTask)
        {
            var count = await countTask;
            if (count > 0)
                items.Add(new FileUsageItem { Label = label, Count = count, IsRequired = false });
        }

        await AddOptionalAsync("Bank logos", _context.Banks.CountAsync(x => x.LogoFileID == id, ct));
        await AddOptionalAsync("Brand logos", _context.Brands.CountAsync(x => x.LogoFileID == id, ct));
        await AddOptionalAsync("Vendor logos", _context.Vendors.CountAsync(x => x.LogoFileID == id, ct));
        await AddOptionalAsync("Member avatars", _context.Members.CountAsync(x => x.AvatarID == id, ct));
        await AddOptionalAsync("Client avatars", _context.WebsiteClients.CountAsync(x => x.AvatarID == id, ct));
        await AddOptionalAsync("Payment receipts", _context.Payments.CountAsync(x => x.ReceiptFileID == id, ct));
        await AddOptionalAsync("Post featured images", _context.Posts.CountAsync(x => x.FeaturedImageFileID == id, ct));
        await AddOptionalAsync("Product featured images", _context.Products.CountAsync(x => x.FeaturedImageFileID == id, ct));
        await AddOptionalAsync("Product category images", _context.ProductCategories.CountAsync(x => x.ImageFileID == id, ct));
        await AddOptionalAsync("Product variant images", _context.ProductVariants.CountAsync(x => x.ImageFileID == id, ct));
        await AddOptionalAsync("Media set items", _context.MediaSetItems.CountAsync(x => x.FileID == id, ct));
        await AddOptionalAsync("Video thumbnails", _context.MediaSetItems.CountAsync(x => x.VideoThumbnailFileID == id, ct));
        await AddOptionalAsync("Slideshow mobile images", _context.SlideshowSlides.CountAsync(x => x.MobileFileID == id, ct));
        await AddOptionalAsync("Website logos", _context.Websites.CountAsync(x => x.LogoFileID == id, ct));
        await AddOptionalAsync("Website favicons", _context.Websites.CountAsync(x => x.FaveIconFileID == id, ct));
        await AddOptionalAsync("Watermark images", _context.WebsiteWatermarkSettings.CountAsync(x => x.WatermarkFileID == id, ct));

        // Required FK: primary slideshow image — deleting the file removes those slides.
        var requiredSlides = await _context.SlideshowSlides.AsNoTracking()
            .Where(s => s.FileID == id)
            .Select(s => new { s.SlideshowSlideID, SlideshowName = s.Slideshow.Name, s.Title })
            .Take(20)
            .ToListAsync(ct);
        if (requiredSlides.Count > 0)
        {
            var total = requiredSlides.Count < 20
                ? requiredSlides.Count
                : await _context.SlideshowSlides.CountAsync(s => s.FileID == id, ct);
            var detail = string.Join(", ", requiredSlides
                .Select(s => string.IsNullOrWhiteSpace(s.Title) ? s.SlideshowName : $"{s.SlideshowName}: {s.Title}")
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Take(8));
            items.Add(new FileUsageItem
            {
                Label = "Slideshow slides (primary image)",
                Count = total,
                Detail = string.IsNullOrWhiteSpace(detail) ? null : detail,
                IsRequired = true,
            });
        }

        return new FileUsageSummary { Items = items };
    }

    /// <summary>
    /// Deletes the file from storage, nulls every optional FK that pointed at it, removes slideshow
    /// slides that required it as their primary image, then hard-deletes the <see cref="FileRecord"/> row.
    /// </summary>
    public async Task SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        var record = await _context.FileRecords
            .Include(f => f.WebsiteStorageSettings)
            .FirstOrDefaultAsync(f => f.FileRecordID == id, ct)
            ?? throw new InvalidOperationException("File not found.");

        // Remove from storage first so a failed backend delete does not leave an orphaned blob
        // while the library already removed the row.
        if (record.WebsiteStorageSettings is not null)
        {
            var setting = record.WebsiteStorageSettings;
            var ctx = ToContext(setting);
            var provider = _providers.Get((StorageProviderType)setting.StorageProvider);

            var mainKey = ResolveStorageKey(record.StoragePath, record.CDNFileCode, record.StoredFileName);
            if (!string.IsNullOrWhiteSpace(mainKey))
                await DeleteFromStorageAsync(provider, ctx, mainKey, ct);

            var thumbKey = ResolveStorageKey(record.ThumbnailStorage, null, null);
            if (!string.IsNullOrWhiteSpace(thumbKey)
                && !string.Equals(thumbKey, mainKey, StringComparison.Ordinal))
            {
                await DeleteFromStorageAsync(provider, ctx, thumbKey, ct);
            }
        }

        await ClearFileReferencesAsync(id, ct);

        // Junction rows (no optional FK — must delete).
        await _context.FileRecordTags.Where(t => t.FileRecordID == id).ExecuteDeleteAsync(ct);

        // Slideshow slides require a primary FileID; without the image the slide is unusable.
        await _context.SlideshowSlides.Where(s => s.FileID == id).ExecuteDeleteAsync(ct);

        _context.FileRecords.Remove(record);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>Sets every optional FileRecord FK to null so the row can be hard-deleted.</summary>
    private async Task ClearFileReferencesAsync(int fileId, CancellationToken ct)
    {
        await _context.Banks.Where(x => x.LogoFileID == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.LogoFileID, (int?)null), ct);
        await _context.Brands.Where(x => x.LogoFileID == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.LogoFileID, (int?)null), ct);
        await _context.Vendors.Where(x => x.LogoFileID == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.LogoFileID, (int?)null), ct);
        await _context.Members.Where(x => x.AvatarID == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.AvatarID, (int?)null), ct);
        await _context.WebsiteClients.Where(x => x.AvatarID == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.AvatarID, (int?)null), ct);
        await _context.Payments.Where(x => x.ReceiptFileID == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ReceiptFileID, (int?)null), ct);
        await _context.Posts.Where(x => x.FeaturedImageFileID == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.FeaturedImageFileID, (int?)null), ct);
        await _context.Products.Where(x => x.FeaturedImageFileID == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.FeaturedImageFileID, (int?)null), ct);
        await _context.ProductCategories.Where(x => x.ImageFileID == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ImageFileID, (int?)null), ct);
        await _context.ProductVariants.Where(x => x.ImageFileID == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ImageFileID, (int?)null), ct);
        await _context.MediaSetItems.Where(x => x.FileID == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.FileID, (int?)null), ct);
        await _context.MediaSetItems.Where(x => x.VideoThumbnailFileID == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.VideoThumbnailFileID, (int?)null), ct);
        await _context.SlideshowSlides.Where(x => x.MobileFileID == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.MobileFileID, (int?)null), ct);
        await _context.Websites.Where(x => x.LogoFileID == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.LogoFileID, (int?)null), ct);
        await _context.Websites.Where(x => x.FaveIconFileID == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.FaveIconFileID, (int?)null), ct);
        await _context.WebsiteWatermarkSettings.Where(x => x.WatermarkFileID == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.WatermarkFileID, (int?)null), ct);
    }

    /// <summary>Picks the best storage object key: path from upload, then CDN code, then stored file name.</summary>
    private static string? ResolveStorageKey(string? storagePath, string? cdnFileCode, string? storedFileName)
    {
        if (!string.IsNullOrWhiteSpace(storagePath)) return storagePath.Trim();
        if (!string.IsNullOrWhiteSpace(cdnFileCode)) return cdnFileCode.Trim();
        if (!string.IsNullOrWhiteSpace(storedFileName)) return storedFileName.Trim();
        return null;
    }

    /// <summary>
    /// Deletes one object; treats "already missing" as success so re-deletes and partial cleanups work.
    /// Other failures bubble up so the admin can see and retry.
    /// </summary>
    private static async Task DeleteFromStorageAsync(
        IFileStorageProvider provider, StorageSettingContext ctx, string storedName, CancellationToken ct)
    {
        try
        {
            await provider.DeleteAsync(ctx, storedName, ct);
        }
        catch (Exception ex) when (IsMissingObjectError(ex))
        {
            // Object already gone — treat as success.
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to delete '{storedName}' from storage: {ex.Message}", ex);
        }
    }

    private static bool IsMissingObjectError(Exception ex)
    {
        // Provider SDKs surface missing objects differently; match common cases without referencing SDK types.
        for (var e = ex; e is not null; e = e.InnerException!)
        {
            var msg = e.Message ?? "";
            if (msg.Contains("404", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("Not Found", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("NoSuchKey", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("not_found", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("BlobNotFound", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("path/not_found", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    // ── Virtual folders ──────────────────────────────────────
    public async Task<IReadOnlyList<FileFolder>> GetFoldersAsync(int websiteId, CancellationToken ct = default) =>
        await _context.FileFolders.AsNoTracking()
            .Where(f => f.WebsiteID == websiteId)
            .OrderBy(f => f.Name)
            .ToListAsync(ct);

    public async Task<PagedResult<FileFolder>> GetFoldersPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.FileFolders.AsNoTracking()
            .Where(f => f.WebsiteID == websiteId);

        if (query.GetSearch(nameof(FileFolder.Name)) is string name)
            q = q.Where(f => f.Name.Contains(name));
        if (query.GetSearch(nameof(FileFolder.Description)) is string description)
            q = q.Where(f => f.Description != null && f.Description.Contains(description));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(FileFolder.Name))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<FileFolder> { Items = items, TotalCount = total };
    }

    public async Task<FileFolder> CreateFolderAsync(int websiteId, string name, string? description, int? parentFolderId = null, CancellationToken ct = default)
    {
        if (parentFolderId is int parentId)
            await EnsureFolderBelongsToWebsiteAsync(parentId, websiteId, ct);

        var folder = new FileFolder
        {
            WebsiteID = websiteId,
            ParentFolderID = parentFolderId,
            Name = Truncate(name, 120)!,
            Description = Truncate(description, 400),
            CreateDate = DateTime.UtcNow,
        };
        _context.FileFolders.Add(folder);
        await _context.SaveChangesAsync(ct);
        return folder;
    }

    public async Task UpdateFolderAsync(int folderId, string name, string? description, int? parentFolderId, CancellationToken ct = default)
    {
        var folder = await _context.FileFolders.FirstOrDefaultAsync(f => f.FileFolderID == folderId, ct)
            ?? throw new InvalidOperationException($"Folder {folderId} not found.");

        if (parentFolderId == folderId)
            throw new InvalidOperationException("A folder cannot be its own parent.");

        if (parentFolderId is int parentId)
        {
            await EnsureFolderBelongsToWebsiteAsync(parentId, folder.WebsiteID, ct);
            if (await IsDescendantAsync(folderId, parentId, ct))
                throw new InvalidOperationException("Cannot move a folder under one of its descendants.");
        }

        folder.Name = Truncate(name, 120)!;
        folder.Description = Truncate(description, 400);
        folder.ParentFolderID = parentFolderId;
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteFolderAsync(int folderId, CancellationToken ct = default)
    {
        var folder = await _context.FileFolders.FirstOrDefaultAsync(f => f.FileFolderID == folderId, ct)
            ?? throw new InvalidOperationException($"Folder {folderId} not found.");

        var parentId = folder.ParentFolderID;

        // Promote children to the deleted folder's parent (or root).
        await _context.FileFolders.Where(f => f.ParentFolderID == folderId)
            .ExecuteUpdateAsync(s => s.SetProperty(f => f.ParentFolderID, parentId), ct);

        // Detach files so they fall back to the virtual root.
        await _context.FileRecords.Where(f => f.FileFolderID == folderId)
            .ExecuteUpdateAsync(s => s.SetProperty(f => f.FileFolderID, (int?)null), ct);

        await _context.FileFolders.Where(f => f.FileFolderID == folderId).ExecuteDeleteAsync(ct);
    }

    private async Task EnsureFolderBelongsToWebsiteAsync(int folderId, int websiteId, CancellationToken ct)
    {
        var ok = await _context.FileFolders.AsNoTracking()
            .AnyAsync(f => f.FileFolderID == folderId && f.WebsiteID == websiteId, ct);
        if (!ok)
            throw new InvalidOperationException($"Folder {folderId} was not found for this website.");
    }

    /// <summary>True when <paramref name="candidateId"/> is under <paramref name="ancestorId"/> in the tree.</summary>
    private async Task<bool> IsDescendantAsync(int ancestorId, int candidateId, CancellationToken ct)
    {
        var parentById = await _context.FileFolders.AsNoTracking()
            .Select(f => new { f.FileFolderID, f.ParentFolderID })
            .ToDictionaryAsync(f => f.FileFolderID, f => f.ParentFolderID, ct);

        var current = (int?)candidateId;
        for (var guard = 0; current is int id && guard < 256; guard++)
        {
            if (id == ancestorId) return true;
            if (!parentById.TryGetValue(id, out var parent)) break;
            current = parent;
        }
        return false;
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
