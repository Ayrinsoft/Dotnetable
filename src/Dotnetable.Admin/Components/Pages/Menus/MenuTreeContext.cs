using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace Dotnetable.Admin.Components.Pages.Menus;

/// <summary>Requests toggling <see cref="Item"/>'s active flag to <see cref="Value"/>.</summary>
public sealed record MenuItemActiveChange(MenuItem Item, bool Value);

/// <summary>Requests moving <see cref="DraggedId"/> under <see cref="NewParentId"/> at sibling <see cref="Index"/>.</summary>
public sealed record MenuItemMoveRequest(int DraggedId, int? NewParentId, int Index);

/// <summary>Shared state cascaded to every <see cref="MenuTreeNode"/> in a <see cref="MenuItemTreeView"/>.</summary>
public sealed class MenuTreeContext
{
    public List<MenuItem> AllItems { get; set; } = new();
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public int? DraggedId { get; set; }
    public EventCallback<MenuItem> OnEdit { get; set; }
    public EventCallback<int?> OnAddChild { get; set; }
    public EventCallback<MenuItem> OnDelete { get; set; }
    public EventCallback<MenuItemActiveChange> OnToggleActive { get; set; }

    /// <summary>Requests moving <c>DraggedId</c> under <c>NewParentId</c> at sibling <c>Index</c>.</summary>
    public Func<int, int?, int, Task>? MoveAsync { get; set; }

    /// <summary>Re-renders the whole tree (used for drag-state highlighting).</summary>
    public Action? NotifyChanged { get; set; }
}
