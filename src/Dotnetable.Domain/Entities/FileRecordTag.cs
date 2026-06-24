using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class FileRecordTag
{
    public int FileRecordTagID { get; set; }

    public int FileRecordID { get; set; }

    public int FileTagID { get; set; }

    public virtual FileRecord FileRecord { get; set; } = null!;

    public virtual FileTag FileTag { get; set; } = null!;
}
