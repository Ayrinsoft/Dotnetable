using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

/// <summary>A rendered slideshow handed to the public website (read-only projection).</summary>
public sealed class SlideshowDto
{
    public int SlideshowID { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? PlacementKey { get; init; }
    public SlideTransitionEffect TransitionEffect { get; init; }
    public bool AutoPlay { get; init; }
    public int IntervalMs { get; init; }
    public bool ShowArrows { get; init; }
    public bool ShowDots { get; init; }
    public bool EnableLightbox { get; init; }
    public string? AspectRatio { get; init; }

    /// <summary>Active slides, already ordered by sort order.</summary>
    public IReadOnlyList<SlideDto> Slides { get; init; } = Array.Empty<SlideDto>();
}

/// <summary>A single rendered slide with its resolved image URLs.</summary>
public sealed class SlideDto
{
    public int SlideshowSlideID { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string? MobileImageUrl { get; init; }
    public string? AltText { get; init; }
    public string? Title { get; init; }
    public string? Caption { get; init; }
    public string? ButtonText { get; init; }
    public string? LinkUrl { get; init; }
    public bool OpenInNewTab { get; init; }
}
