using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class LocalizationValue
{
    public int LocalizationValueID { get; set; }

    public int LocalizationKeyID { get; set; }

    public string ItemValue { get; set; } = null!;

    public string LanguageCode { get; set; } = null!;

    public virtual LocalizationKey LocalizationKey { get; set; } = null!;
}
