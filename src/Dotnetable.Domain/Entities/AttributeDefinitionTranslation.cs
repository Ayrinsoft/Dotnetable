using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class AttributeDefinitionTranslation
{
    public int AttributeDefinitionTranslationID { get; set; }

    public int AttributeDefinitionID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Unit { get; set; }

    public virtual AttributeDefinition AttributeDefinition { get; set; } = null!;
}
