using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class CityTranslation
{
    public int CityTranslationID { get; set; }

    public int CityID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public virtual City City { get; set; } = null!;
}
