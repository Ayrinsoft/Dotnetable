using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductCategory
{
    public int ProductCategoryID { get; set; }

    public int WebsiteID { get; set; }

    public int? ParentCategoryID { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int? ImageFileID { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual FileRecord? ImageFile { get; set; }

    public virtual ICollection<ProductCategory> InverseParentCategory { get; set; } = new List<ProductCategory>();

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual ProductCategory? ParentCategory { get; set; }

    public virtual ICollection<ProductCategoryAttribute> ProductCategoryAttributes { get; set; } = new List<ProductCategoryAttribute>();

    public virtual ICollection<MarketplaceChannelCategory> MarketplaceChannelCategories { get; set; } = new List<MarketplaceChannelCategory>();

    public virtual ICollection<ProductCategoryMap> ProductCategoryMaps { get; set; } = new List<ProductCategoryMap>();

    public virtual ICollection<ProductCategoryRelation> ProductCategoryRelations { get; set; } = new List<ProductCategoryRelation>();

    public virtual ICollection<PriceList> PriceLists { get; set; } = new List<PriceList>();

    public virtual ICollection<ProductCategoryTranslation> ProductCategoryTranslations { get; set; } = new List<ProductCategoryTranslation>();

    public virtual Website Website { get; set; } = null!;
}
