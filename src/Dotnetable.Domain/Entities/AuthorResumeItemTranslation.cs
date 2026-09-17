using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>Per-language overrides of an <see cref="AuthorResumeItem"/>'s text; blank fields fall back to the base row.</summary>
public partial class AuthorResumeItemTranslation
{
    public int AuthorResumeItemTranslationID { get; set; }

    public int AuthorResumeItemID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string? Title { get; set; }

    public string? Organization { get; set; }

    public string? Location { get; set; }

    public string? Description { get; set; }

    public virtual AuthorResumeItem AuthorResumeItem { get; set; } = null!;
}
