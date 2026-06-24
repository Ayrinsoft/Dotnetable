using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

/// <summary>Filters applied to the media-library listing on top of paging/sorting.</summary>
public sealed class FileFilter
{
    public FileCategory? Category { get; set; }
    public int? AlbumID { get; set; }
    public int? TagID { get; set; }

    /// <summary>Free-text match against original file name / title.</summary>
    public string? Search { get; set; }
}
