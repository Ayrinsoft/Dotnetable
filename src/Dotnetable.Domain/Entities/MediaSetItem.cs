using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class MediaSetItem
{
    public int MediaSetItemID { get; set; }

    public int MediaSetID { get; set; }

    public int? FileID { get; set; }

    public int? VideoThumbnailFileID { get; set; }

    public string? ExternalVideoUrl { get; set; }

    public int SortOrder { get; set; }

    public virtual FileRecord? File { get; set; }

    public virtual MediaSet MediaSet { get; set; } = null!;

    public virtual FileRecord? VideoThumbnailFile { get; set; }
}
