using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class MenuItem
{
    public int MenuItemID { get; set; }

    public int MenuID { get; set; }

    public int? ParentItemID { get; set; }

    public byte ItemType { get; set; }

    public int? PageID { get; set; }

    public int? PostID { get; set; }

    public int? CategoryID { get; set; }

    public int? ProductID { get; set; }

    public int? ProductCategoryID { get; set; }

    public int? BrandID { get; set; }

    public int? VendorID { get; set; }

    public string? Url { get; set; }

    public string Title { get; set; } = null!;

    public string? Icon { get; set; }

    public string? CssClass { get; set; }

    public bool OpenInNewTab { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<MenuItem> InverseParentItem { get; set; } = new List<MenuItem>();

    public virtual Menu Menu { get; set; } = null!;

    public virtual ICollection<MenuItemTranslation> MenuItemTranslations { get; set; } = new List<MenuItemTranslation>();

    public virtual MenuItem? ParentItem { get; set; }

    public virtual Post? Post { get; set; }
}
