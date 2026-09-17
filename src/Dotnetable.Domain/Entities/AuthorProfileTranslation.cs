using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>Per-language overrides of an <see cref="AuthorProfile"/>'s text; blank fields fall back to the base row.</summary>
public partial class AuthorProfileTranslation
{
    public int AuthorProfileTranslationID { get; set; }

    public int AuthorProfileID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string? Headline { get; set; }

    public string? Bio { get; set; }

    public string? About { get; set; }

    public virtual AuthorProfile AuthorProfile { get; set; } = null!;
}
