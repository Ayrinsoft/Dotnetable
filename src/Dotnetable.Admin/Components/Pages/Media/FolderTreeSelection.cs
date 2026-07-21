using Dotnetable.Application.DTOs;

namespace Dotnetable.Admin.Components.Pages.Media;

/// <summary>Current selection in the media virtual-folder tree.</summary>
public sealed class FolderTreeSelection
{
    public FileFolderScope Scope { get; init; } = FileFolderScope.All;
    public int? FolderId { get; init; }

    public static FolderTreeSelection All { get; } = new() { Scope = FileFolderScope.All };
    public static FolderTreeSelection Root { get; } = new() { Scope = FileFolderScope.Root };

    public static FolderTreeSelection Folder(int folderId) =>
        new() { Scope = FileFolderScope.Folder, FolderId = folderId };
}
