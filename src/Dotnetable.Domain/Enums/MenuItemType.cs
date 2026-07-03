namespace Dotnetable.Domain.Enums;

/// <summary>
/// What a <see cref="Entities.MenuItem"/> links to. Stored as a TINYINT in
/// <see cref="Entities.MenuItem.ItemType"/>. Each typed value pairs with the matching
/// target id column on the item (e.g. <see cref="Custom"/> → <c>Url</c>,
/// <see cref="Post"/> → <c>PostID</c>). Types whose backing content module does not
/// exist yet still store their target id and fall back to the item's <c>Url</c> when rendered.
/// </summary>
public enum MenuItemType : byte
{
    /// <summary>A free-form external or internal link stored in <c>Url</c>.</summary>
    Custom = 0,

    /// <summary>A CMS page (<c>PageID</c>).</summary>
    Page = 1,

    /// <summary>A blog post (<c>PostID</c>).</summary>
    Post = 2,

    /// <summary>A blog / content category (<c>CategoryID</c>).</summary>
    Category = 3,

    /// <summary>A shop product (<c>ProductID</c>).</summary>
    Product = 4,

    /// <summary>A shop product category (<c>ProductCategoryID</c>).</summary>
    ProductCategory = 5,

    /// <summary>A product brand (<c>BrandID</c>).</summary>
    Brand = 6,

    /// <summary>A vendor / seller (<c>VendorID</c>).</summary>
    Vendor = 7,
}
