using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Slideshow
{
    public int SlideshowID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public string? PlacementKey { get; set; }

    public byte TransitionEffect { get; set; } = (byte)1;

    public bool AutoPlay { get; set; } = true;

    public int IntervalMs { get; set; } = 5000;

    public bool ShowArrows { get; set; } = true;

    public bool ShowDots { get; set; } = true;

    public bool EnableLightbox { get; set; } = true;

    public string? AspectRatio { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<SlideshowSlide> SlideshowSlides { get; set; } = new List<SlideshowSlide>();

    public virtual Website Website { get; set; } = null!;
}
