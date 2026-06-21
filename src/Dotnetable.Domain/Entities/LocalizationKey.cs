using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class LocalizationKey
{
    public int LocalizationKeyID { get; set; }

    public string ItemKey { get; set; } = null!;

    public string DefaultValue { get; set; } = null!;

    public int WebsiteID { get; set; }

    public virtual ICollection<LocalizationValue> LocalizationValues { get; set; } = new List<LocalizationValue>();

    public virtual Language Website { get; set; } = null!;
}
