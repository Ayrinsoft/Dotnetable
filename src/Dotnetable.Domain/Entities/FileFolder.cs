using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class FileFolder
{
    public int FileFolderID { get; set; }

    public int WebsiteID { get; set; }

    public int? ParentFolderID { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual ICollection<FileRecord> FileRecords { get; set; } = new List<FileRecord>();

    public virtual ICollection<FileFolder> InverseParentFolder { get; set; } = new List<FileFolder>();

    public virtual FileFolder? ParentFolder { get; set; }

    public virtual Website Website { get; set; } = null!;
}
