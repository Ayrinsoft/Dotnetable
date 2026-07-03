using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

/// <summary>A rendered navigation menu handed to the public website (read-only projection).</summary>
public sealed class MenuDto
{
    public int MenuID { get; init; }
    public string Name { get; init; } = string.Empty;
    public MenuLocation Location { get; init; }

    /// <summary>Top-level items (each may carry nested <see cref="MenuItemDto.Children"/>), already
    /// filtered to active items and ordered by sort order.</summary>
    public IReadOnlyList<MenuItemDto> Items { get; init; } = Array.Empty<MenuItemDto>();
}

/// <summary>A single rendered menu entry with its resolved link target and any child entries.</summary>
public sealed class MenuItemDto
{
    public int MenuItemID { get; init; }

    /// <summary>Display text, already localized to the requested language when a translation exists.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Final href to render. Resolved from the item type/target, falling back to its custom Url.</summary>
    public string Url { get; init; } = "#";

    public string? Icon { get; init; }
    public string? CssClass { get; init; }
    public bool OpenInNewTab { get; init; }

    public IReadOnlyList<MenuItemDto> Children { get; init; } = Array.Empty<MenuItemDto>();
}
