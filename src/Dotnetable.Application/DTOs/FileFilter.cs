using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

/// <summary>Filters applied to the media-library listing on top of paging/sorting.</summary>
public sealed class FileFilter
{
    public FileCategory? Category { get; set; }

    /// <summary>Legacy single-album filter. Prefer <see cref="AlbumIDs"/> when selecting multiple.</summary>
    public int? AlbumID { get; set; }

    /// <summary>Legacy single-tag filter. Prefer <see cref="TagIDs"/> when selecting multiple.</summary>
    public int? TagID { get; set; }

    /// <summary>When non-empty, only files in any of these albums are returned (OR).</summary>
    public IReadOnlyList<int>? AlbumIDs { get; set; }

    /// <summary>When non-empty, only files that have any of these tags are returned (OR).</summary>
    public IReadOnlyList<int>? TagIDs { get; set; }

    /// <summary>Free-text match against original file name / title.</summary>
    public string? Search { get; set; }

    /// <summary>Effective album ids after merging <see cref="AlbumIDs"/> and <see cref="AlbumID"/>.</summary>
    public IReadOnlyList<int> EffectiveAlbumIDs()
    {
        if (AlbumIDs is { Count: > 0 })
            return AlbumIDs;
        return AlbumID is int id ? new[] { id } : Array.Empty<int>();
    }

    /// <summary>Effective tag ids after merging <see cref="TagIDs"/> and <see cref="TagID"/>.</summary>
    public IReadOnlyList<int> EffectiveTagIDs()
    {
        if (TagIDs is { Count: > 0 })
            return TagIDs;
        return TagID is int id ? new[] { id } : Array.Empty<int>();
    }
}
