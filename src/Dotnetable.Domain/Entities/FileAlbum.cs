using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class FileAlbum
{
    public int FileAlbumID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual ICollection<FileRecord> FileRecords { get; set; } = new List<FileRecord>();

    public virtual Website Website { get; set; } = null!;
}
