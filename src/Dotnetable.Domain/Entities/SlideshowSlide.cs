using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class SlideshowSlide
{
    public int SlideshowSlideID { get; set; }

    public int SlideshowID { get; set; }

    public int FileID { get; set; }

    public int? MobileFileID { get; set; }

    public string? Title { get; set; }

    public string? Caption { get; set; }

    public string? ButtonText { get; set; }

    public string? LinkUrl { get; set; }

    public bool OpenInNewTab { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime? StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public virtual FileRecord File { get; set; } = null!;

    public virtual FileRecord? MobileFile { get; set; }

    public virtual Slideshow Slideshow { get; set; } = null!;
}
