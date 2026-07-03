using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class PostTranslation
{
    public int PostTranslationID { get; set; }

    public int PostID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Excerpt { get; set; }

    public string? Content { get; set; }

    public virtual Post Post { get; set; } = null!;
}
