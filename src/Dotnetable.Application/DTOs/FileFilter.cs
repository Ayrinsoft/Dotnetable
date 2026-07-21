using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

/// <summary>How the media listing scopes files by virtual folder.</summary>
public enum FileFolderScope
{
    /// <summary>No folder filter (entire library).</summary>
    All = 0,

    /// <summary>Only files with no folder (virtual root).</summary>
    Root = 1,

    /// <summary>Only files directly inside <see cref="FileFilter.FolderID"/>.</summary>
    Folder = 2,
}

/// <summary>Filters applied to the media-library listing on top of paging/sorting.</summary>
public sealed class FileFilter
{
    public FileCategory? Category { get; set; }

    /// <summary>Folder filter mode. Default is <see cref="FileFolderScope.All"/>.</summary>
    public FileFolderScope FolderScope { get; set; } = FileFolderScope.All;

    /// <summary>Used when <see cref="FolderScope"/> is <see cref="FileFolderScope.Folder"/>.</summary>
    public int? FolderID { get; set; }

    /// <summary>Legacy single-tag filter. Prefer <see cref="TagIDs"/> when selecting multiple.</summary>
    public int? TagID { get; set; }

    /// <summary>When non-empty, only files that have any of these tags are returned (OR).</summary>
    public IReadOnlyList<int>? TagIDs { get; set; }

    /// <summary>Free-text match against original file name / title.</summary>
    public string? Search { get; set; }

    /// <summary>Effective tag ids after merging <see cref="TagIDs"/> and <see cref="TagID"/>.</summary>
    public IReadOnlyList<int> EffectiveTagIDs()
    {
        if (TagIDs is { Count: > 0 })
            return TagIDs;
        return TagID is int id ? new[] { id } : Array.Empty<int>();
    }
}
