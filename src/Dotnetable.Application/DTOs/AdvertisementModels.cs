using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

/// <summary>A rendered text advertisement handed to the public website (read-only projection).</summary>
public sealed class AdvertisementDto
{
    public int AdvertisementID { get; init; }
    public AdvertisementLocation Location { get; init; }

    /// <summary>Clickable text, already localized to the requested language when a translation exists.</summary>
    public string Keyword { get; init; } = string.Empty;

    /// <summary>Final href. Localized URL when present, otherwise the default-language URL.</summary>
    public string Url { get; init; } = "#";

    public bool OpenInNewTab { get; init; }
}
