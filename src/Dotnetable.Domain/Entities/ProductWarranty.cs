using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductWarranty
{
    public int ProductWarrantyID { get; set; }

    public int ProductID { get; set; }

    public int? WarrantyID { get; set; }

    public string? CustomTitle { get; set; }

    public string? CustomDescription { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Warranty? Warranty { get; set; }
}
