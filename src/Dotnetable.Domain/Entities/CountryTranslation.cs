using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class CountryTranslation
{
    public int CountryTranslationID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public int CountryID { get; set; }

    public virtual Country Country { get; set; } = null!;
}
