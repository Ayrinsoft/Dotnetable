using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WebstieStorageSetting
{
    public int WebsiteStorageSettingsID { get; set; }

    public int WebsiteID { get; set; }

    public short StorageProvider { get; set; }

    public string StorageSettingsJSON { get; set; } = null!;

    public bool Active { get; set; }

    public long MaxFileSizeKB { get; set; }

    public string? AllowedExtensions { get; set; }

    public bool AutoGenerateThumbnails { get; set; }

    public virtual ICollection<FileRecord> FileRecords { get; set; } = new List<FileRecord>();

    public virtual Website Website { get; set; } = null!;
}
