using Dotnetable.Application.Instagram;

namespace Dotnetable.Infrastructure.Instagram;

public sealed class PreparedSlide
{
    public required string Url { get; init; }
    public bool IsVideo { get; init; }
}

/// <summary>What will be created on Instagram after covers are ordered.</summary>
public sealed class InstagramDelivery
{
    public required string Caption { get; init; }
    public bool IsReel { get; init; }
    public bool IsCarousel { get; init; }
    public required IReadOnlyList<PreparedSlide> Slides { get; init; }
    public string? VideoCoverUrl { get; init; }
}

public static class InstagramPublishPlanner
{
    public const int MaxSlides = 10;
    public const int MaxCaptionLength = 2200;

    public static InstagramDelivery Plan(string caption, IReadOnlyList<PreparedSlide> slides, int coverIndex, string? videoCoverUrl)
    {
        var text = caption.Trim();
        if (text.Length > MaxCaptionLength)
            throw new InstagramException($"The caption is longer than {MaxCaptionLength} characters.");
        if (slides.Count < 1)
            throw new InstagramException("Add a photo or a video.");
        if (slides.Count > MaxSlides)
            throw new InstagramException($"Instagram accepts at most {MaxSlides} items.");

        if (slides.Count == 1 && slides[0].IsVideo)
        {
            return new InstagramDelivery
            {
                Caption = text,
                IsReel = true,
                Slides = slides,
                VideoCoverUrl = string.IsNullOrWhiteSpace(videoCoverUrl) ? null : videoCoverUrl.Trim(),
            };
        }

        var ordered = slides.ToList();
        var cover = Math.Clamp(coverIndex, 0, ordered.Count - 1);
        if (cover != 0)
        {
            var chosen = ordered[cover];
            ordered.RemoveAt(cover);
            ordered.Insert(0, chosen);
        }

        return new InstagramDelivery
        {
            Caption = text,
            IsCarousel = ordered.Count > 1,
            Slides = ordered,
        };
    }
}
