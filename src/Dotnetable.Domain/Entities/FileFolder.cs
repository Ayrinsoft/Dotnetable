using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Virtual media-library folder. Nested via <see cref="ParentFolderID"/>;
/// files with null <c>FileFolderID</c> live under the virtual root.
/// </summary>
public partial class FileFolder
{
    public int FileFolderID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Null = top-level folder under the virtual root.</summary>
    public int? ParentFolderID { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual ICollection<FileFolder> InverseParentFolder { get; set; } = new List<FileFolder>();

    public virtual FileFolder? ParentFolder { get; set; }

    public virtual ICollection<FileRecord> FileRecords { get; set; } = new List<FileRecord>();

    public virtual Website Website { get; set; } = null!;
}
