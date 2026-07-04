namespace Dotnetable.Domain.Enums;

/// <summary>
/// Animation used when the public website transitions between slides.
/// Stored as a TINYINT in <see cref="Entities.Slideshow.TransitionEffect"/>.
/// </summary>
public enum SlideTransitionEffect : byte
{
    /// <summary>Slides horizontally from one slide to the next.</summary>
    Slide = 1,

    /// <summary>Cross-fades between slides.</summary>
    Fade = 2,
}
