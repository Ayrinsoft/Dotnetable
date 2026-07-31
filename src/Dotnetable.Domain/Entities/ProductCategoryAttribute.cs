using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Links a product category to a catalog attribute definition (e.g. TVs → Screen size, Resolution).
/// Products in that category show these attributes by default; empty values are not stored on the product.
/// </summary>
public partial class ProductCategoryAttribute
{
    public int ProductCategoryID { get; set; }

    public int AttributeDefinitionID { get; set; }

    public int SortOrder { get; set; }

    public virtual AttributeDefinition AttributeDefinition { get; set; } = null!;

    public virtual ProductCategory ProductCategory { get; set; } = null!;
}
