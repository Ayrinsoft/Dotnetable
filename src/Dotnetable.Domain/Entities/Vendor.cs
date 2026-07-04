using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Vendor
{
    public int VendorID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int? LogoFileID { get; set; }

    public decimal Rating { get; set; }

    public bool IsActive { get; set; }

    public virtual FileRecord? LogoFile { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductAnswer> ProductAnswers { get; set; } = new List<ProductAnswer>();

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual ICollection<VendorProduct> VendorProducts { get; set; } = new List<VendorProduct>();

    public virtual ICollection<VendorTranslation> VendorTranslations { get; set; } = new List<VendorTranslation>();

    public virtual Website Website { get; set; } = null!;
}
