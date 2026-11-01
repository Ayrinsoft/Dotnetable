using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class PostType
{
    public int PostTypeID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public bool HasCategories { get; set; }

    public bool HasTags { get; set; }

    public bool HasAuthor { get; set; }

    public bool CommentsEnabled { get; set; }

    /// <summary>
    /// When true, posts of this type may set <see cref="Post.StableKey"/>.
    /// Blog-like types leave this false so the editor hides the field.
    /// </summary>
    public bool SupportsStableKey { get; set; }

    /// <summary>
    /// When true, posts of this type may publish release-scoped editions
    /// (<see cref="Post.AppliesFrom"/> / <see cref="Post.AppliesTo"/>) that share one stable key.
    /// Also allows a stable key. Off by default.
    /// </summary>
    public bool SupportsVersioning { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual Website Website { get; set; } = null!;
}
