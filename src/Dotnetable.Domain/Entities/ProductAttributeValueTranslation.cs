using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductAttributeValueTranslation
{
    public int ProductAttributeValueTranslationID { get; set; }

    public int ProductAttributeValueID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string CustomValue { get; set; } = null!;

    public virtual ProductAttributeValue ProductAttributeValue { get; set; } = null!;
}
