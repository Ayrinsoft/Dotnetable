using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class AttributeDefinition
{
    public int AttributeDefinitionID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public byte InputType { get; set; }

    public string? Unit { get; set; }

    public bool IsFilterable { get; set; }

    public bool IsVariantAttribute { get; set; }

    public bool IsComparable { get; set; }

    public int SortOrder { get; set; }

    public bool ShowOnTop { get; set; }

    public bool Active { get; set; }

    public virtual ICollection<AttributeDefinitionTranslation> AttributeDefinitionTranslations { get; set; } = new List<AttributeDefinitionTranslation>();

    public virtual ICollection<AttributeOption> AttributeOptions { get; set; } = new List<AttributeOption>();

    public virtual Website Website { get; set; } = null!;
}
