using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Category
{
    public int CategoryID { get; set; }

    public int WebsiteID { get; set; }

    public int? PostTypeID { get; set; }

    public int? ParentCategoryID { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<CategoryTranslation> CategoryTranslations { get; set; } = new List<CategoryTranslation>();

    public virtual ICollection<Category> InverseParentCategory { get; set; } = new List<Category>();

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual Category? ParentCategory { get; set; }

    public virtual ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();

    public virtual PostType? PostType { get; set; }

    public virtual Website Website { get; set; } = null!;
}
