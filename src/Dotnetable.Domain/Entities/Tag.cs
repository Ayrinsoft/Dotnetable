using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Tag
{
    public int TagID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public virtual ICollection<TagTranslation> TagTranslations { get; set; } = new List<TagTranslation>();

    public virtual Website Website { get; set; } = null!;

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
