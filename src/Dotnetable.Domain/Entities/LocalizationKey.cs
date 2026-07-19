using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class LocalizationKey
{
    public int LocalizationKeyID { get; set; }

    public string ItemKey { get; set; } = null!;

    public string DefaultValue { get; set; } = null!;

    /// <summary>
    /// <c>null</c> = admin panel UI strings (Initial Data → Admin Translations).
    /// Non-null = that website's storefront/theme keys only (Website → Translations).
    /// </summary>
    public int? WebsiteID { get; set; }

    public virtual ICollection<LocalizationValue> LocalizationValues { get; set; } = new List<LocalizationValue>();

    public virtual Website? Website { get; set; }
}
