using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class PostType
{
    public int PostTypeID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public bool HasCategories { get; set; } = true;

    public bool HasTags { get; set; } = true;

    public bool HasAuthor { get; set; } = true;

    public bool CommentsEnabled { get; set; } = true;

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual Website Website { get; set; } = null!;
}
