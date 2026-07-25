using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductCategoryRelation
{
    public int ProductID { get; set; }

    public int RelatedProductCategoryID { get; set; }

    public byte RelationType { get; set; }

    public int MaxItems { get; set; } = 10;

    public virtual Product Product { get; set; } = null!;

    public virtual ProductCategory RelatedProductCategory { get; set; } = null!;
}
