using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class CategoryTranslation
{
    public int CategoryTranslationID { get; set; }

    public int CategoryID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;
}
