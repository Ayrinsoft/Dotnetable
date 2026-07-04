using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductCategoryMap
{
    public int ProductID { get; set; }

    public int ProductCategoryID { get; set; }

    public bool IsPrimary { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ProductCategory ProductCategory { get; set; } = null!;
}
