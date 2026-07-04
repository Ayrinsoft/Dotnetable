using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductAttributeValue
{
    public int ProductAttributeValueID { get; set; }

    public int ProductID { get; set; }

    public int AttributeDefinitionID { get; set; }

    public int? AttributeOptionID { get; set; }

    public string? CustomValue { get; set; }

    public decimal? NumericValue { get; set; }

    public bool IsFeatured { get; set; }

    public int SortOrder { get; set; }

    public virtual AttributeDefinition AttributeDefinition { get; set; } = null!;

    public virtual AttributeOption? AttributeOption { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductAttributeValueTranslation> ProductAttributeValueTranslations { get; set; } = new List<ProductAttributeValueTranslation>();
}
