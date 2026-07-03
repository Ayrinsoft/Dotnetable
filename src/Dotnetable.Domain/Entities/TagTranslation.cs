using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class TagTranslation
{
    public int TagTranslationID { get; set; }

    public int TagID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public virtual Tag Tag { get; set; } = null!;
}
