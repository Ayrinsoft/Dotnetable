using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class VariantAttributeValue
{
    public int ProductVariantID { get; set; }

    public int AttributeDefinitionID { get; set; }

    public int AttributeOptionID { get; set; }

    public virtual AttributeDefinition AttributeDefinition { get; set; } = null!;

    public virtual AttributeOption AttributeOption { get; set; } = null!;

    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
