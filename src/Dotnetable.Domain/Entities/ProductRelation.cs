using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductRelation
{
    public int ProductID { get; set; }

    public int RelatedProductID { get; set; }

    public byte RelationType { get; set; }

    public int SortOrder { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Product RelatedProduct { get; set; } = null!;
}
