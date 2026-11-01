namespace Dotnetable.Domain.Entities;

/// <summary>
/// A product category a website has chosen to show. Rows apply only when
/// <see cref="Website.LimitVisibleCategories"/> is true. Ancestors of a chosen
/// category stay in the menu so the path can be opened; their products do not,
/// unless that ancestor is chosen too.
/// </summary>
public partial class WebsiteVisibleProductCategory
{
    public int WebsiteVisibleProductCategoryID { get; set; }

    public int WebsiteID { get; set; }

    public int ProductCategoryID { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual ProductCategory ProductCategory { get; set; } = null!;
}
