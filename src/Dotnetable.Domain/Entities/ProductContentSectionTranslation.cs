using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductContentSectionTranslation
{
    public int ProductContentSectionTranslationID { get; set; }

    public int ProductContentSectionID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string HtmlContent { get; set; } = null!;

    public virtual ProductContentSection ProductContentSection { get; set; } = null!;
}
