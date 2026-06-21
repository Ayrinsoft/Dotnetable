using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Language
{
    public int LangaugeID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string LanguageCodeISO { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int Priority { get; set; }

    public bool IsDefault { get; set; }

    public bool Active { get; set; }

    public bool RTLDesign { get; set; }

    public int WebsiteID { get; set; }

    public virtual ICollection<LocalizationKey> LocalizationKeys { get; set; } = new List<LocalizationKey>();

    public virtual Website Website { get; set; } = null!;
}
