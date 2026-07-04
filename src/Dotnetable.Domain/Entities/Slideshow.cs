using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Slideshow
{
    public int SlideshowID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    /// <summary>Named placement zone (e.g. "home_top", "category_sidebar"), or null when the
    /// slideshow is only embedded manually via its <c>[slideshow:ID]</c> shortcode.</summary>
    public string? PlacementKey { get; set; }

    public byte TransitionEffect { get; set; }

    public bool AutoPlay { get; set; }

    public int IntervalMs { get; set; }

    public bool ShowArrows { get; set; }

    public bool ShowDots { get; set; }

    public bool EnableLightbox { get; set; }

    public string? AspectRatio { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<SlideshowSlide> SlideshowSlides { get; set; } = new List<SlideshowSlide>();

    public virtual Website Website { get; set; } = null!;
}
