using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class FileTag
{
    public int FileTagID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<FileRecordTag> FileRecordTags { get; set; } = new List<FileRecordTag>();

    public virtual Website Website { get; set; } = null!;
}
