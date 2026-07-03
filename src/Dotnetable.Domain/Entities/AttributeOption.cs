using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class AttributeOption
{
    public int AttributeOptionID { get; set; }

    public int AttributeDefinitionID { get; set; }

    public string Value { get; set; } = null!;

    public string? ColorHex { get; set; }

    public int SortOrder { get; set; }

    public virtual AttributeDefinition AttributeDefinition { get; set; } = null!;

    public virtual ICollection<AttributeOptionTranslation> AttributeOptionTranslations { get; set; } = new List<AttributeOptionTranslation>();
}
