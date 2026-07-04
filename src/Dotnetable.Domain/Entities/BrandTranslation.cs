using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class BrandTranslation
{
    public int BrandTranslationID { get; set; }

    public int BrandID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public virtual Brand Brand { get; set; } = null!;
}
