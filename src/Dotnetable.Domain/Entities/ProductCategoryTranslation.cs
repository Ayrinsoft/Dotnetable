using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductCategoryTranslation
{
    public int ProductCategoryTranslationID { get; set; }

    public int ProductCategoryID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public virtual ProductCategory ProductCategory { get; set; } = null!;
}
