using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductTranslation
{
    public int ProductTranslationID { get; set; }

    public int ProductID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? ShortDescription { get; set; }

    public virtual Product Product { get; set; } = null!;
}
