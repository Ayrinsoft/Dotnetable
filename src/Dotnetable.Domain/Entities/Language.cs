using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Language
{
    public int LanguageID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string LanguageCodeISO { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int Priority { get; set; }

    public bool IsDefault { get; set; }

    public bool Active { get; set; }

    public bool RTLDesign { get; set; }

    /// <summary>
    /// <c>null</c> = admin panel language catalog (Initial Data → Languages, admin UI switcher).
    /// Non-null = languages for that website only (Website → Languages, storefront, content translations).
    /// </summary>
    public int? WebsiteID { get; set; }

    public virtual Website? Website { get; set; }
}
