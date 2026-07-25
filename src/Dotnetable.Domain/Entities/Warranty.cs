using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Warranty
{
    public int WarrantyID { get; set; }

    public int WebsiteID { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? ProviderName { get; set; }

    public int? DurationMonths { get; set; }

    public bool IsActive { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<ProductWarranty> ProductWarranties { get; set; } = new List<ProductWarranty>();

    public virtual ICollection<WarrantyTranslation> WarrantyTranslations { get; set; } = new List<WarrantyTranslation>();

    public virtual Website Website { get; set; } = null!;
}
