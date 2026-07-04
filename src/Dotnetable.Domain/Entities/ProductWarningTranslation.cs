using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductWarningTranslation
{
    public int ProductWarningTranslationID { get; set; }

    public int ProductWarningID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Text { get; set; } = null!;

    public virtual ProductWarning ProductWarning { get; set; } = null!;
}
