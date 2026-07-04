using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductContentSection
{
    public int ProductContentSectionID { get; set; }

    public int ProductID { get; set; }

    public byte SectionType { get; set; }

    public string? HtmlContent { get; set; }

    public int? FileId { get; set; }

    public int? MediaSetID { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual MediaSet? MediaSet { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductContentSectionTranslation> ProductContentSectionTranslations { get; set; } = new List<ProductContentSectionTranslation>();
}
