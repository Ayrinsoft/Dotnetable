using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductMedium
{
    public int ProductID { get; set; }

    public int MediaSetID { get; set; }

    public int SortOrder { get; set; }

    public virtual MediaSet MediaSet { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
