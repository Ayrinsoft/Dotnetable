using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class FileRecord
{
    public int FileRecordID { get; set; }

    public int WebsiteStorageSettingsID { get; set; }

    public short StorageProvider { get; set; }

    public string? StoragePath { get; set; }

    public string? CNDUrl { get; set; }

    public string OriginalFileName { get; set; } = null!;

    public string StoredFileName { get; set; } = null!;

    public string MimeType { get; set; } = null!;

    public int FileSizeKB { get; set; }

    public string? MetadataJSON { get; set; }

    public string? AltText { get; set; }

    public string? Title { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime UploadDate { get; set; }

    public string? ThumbnailStorage { get; set; }

    public string? ThumbnailCDN { get; set; }

    public byte FileCategory { get; set; }

    public string? CDNFileCode { get; set; }

    public int? FileAlbumID { get; set; }

    public virtual FileAlbum? FileAlbum { get; set; }

    public virtual ICollection<FileRecordTag> FileRecordTags { get; set; } = new List<FileRecordTag>();

    public virtual ICollection<Website> WebsiteFaveIconFiles { get; set; } = new List<Website>();

    public virtual ICollection<Website> WebsiteLogoFiles { get; set; } = new List<Website>();

    public virtual WebstieStorageSetting WebsiteStorageSettings { get; set; } = null!;
}
