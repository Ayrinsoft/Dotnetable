using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class PageTranslation
{
    public int PageTranslationID { get; set; }

    public int PageID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Content { get; set; }

    public virtual Page Page { get; set; } = null!;
}
