using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Brand
{
    public int BrandID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int? LogoFileID { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<BrandTranslation> BrandTranslations { get; set; } = new List<BrandTranslation>();

    public virtual FileRecord? LogoFile { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual ICollection<PriceList> PriceLists { get; set; } = new List<PriceList>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual Website Website { get; set; } = null!;
}
