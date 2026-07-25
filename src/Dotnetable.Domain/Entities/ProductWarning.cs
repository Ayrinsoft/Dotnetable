using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductWarning
{
    public int ProductWarningID { get; set; }

    public int ProductID { get; set; }

    public string Severity { get; set; } = "info";

    public string Text { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductWarningTranslation> ProductWarningTranslations { get; set; } = new List<ProductWarningTranslation>();
}
