using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class AttributeOptionTranslation
{
    public int AttributeOptionTranslationID { get; set; }

    public int AttributeOptionID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Value { get; set; } = null!;

    public virtual AttributeOption AttributeOption { get; set; } = null!;
}
