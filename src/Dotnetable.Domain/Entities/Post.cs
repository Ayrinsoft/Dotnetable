using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Post
{
    public int PostID { get; set; }

    public int WebsiteID { get; set; }

    public int PostTypeID { get; set; }

    public int? AuthorMemberID { get; set; }

    public string Slug { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Excerpt { get; set; }

    public string? Content { get; set; }

    /// <summary>Overrides the browser-tab/search-result title for this post; falls back to
    /// <see cref="Title"/> plus the site's default when blank.</summary>
    public string? MetaTitle { get; set; }

    /// <summary>Overrides the search-result/social-preview description; falls back to
    /// <see cref="Excerpt"/> when blank.</summary>
    public string? MetaDescription { get; set; }

    public string? MetaKeywords { get; set; }

    public int? FeaturedImageFileID { get; set; }

    public byte Status { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public bool IsFeatured { get; set; }

    public int ViewCount { get; set; }

    /// <summary>Number of signed-in customers who liked this post (once per customer).</summary>
    public int LikeCount { get; set; }

    public bool CommentsEnabled { get; set; }

    /// <summary>When true, <see cref="PostLocation"/> holds this post's map point. The row is removed when this is turned off.</summary>
    public bool HasLocation { get; set; }

    /// <summary>
    /// Stable reference for this logical post. Links keep working when the slug or title changes.
    /// Null on ordinary posts. Set only when the post type allows a stable key or versioning.
    /// Once the post is published, the key does not change.
    /// </summary>
    public string? StableKey { get; set; }

    /// <summary>
    /// First content release this edition covers, such as <c>1.0</c>. Null means every release
    /// (or an unversioned post). Several editions share <see cref="StableKey"/>; unchanged text stays one row.
    /// </summary>
    public string? AppliesFrom { get; set; }

    /// <summary>
    /// Last content release this edition covers, inclusive. Null means the edition is still open.
    /// A later edition with the same <see cref="StableKey"/> wins when both match a release.
    /// </summary>
    public string? AppliesTo { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Member? AuthorMember { get; set; }

    public virtual ICollection<ContentComment> ContentComments { get; set; } = new List<ContentComment>();

    public virtual FileRecord? FeaturedImageFile { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();

    public virtual ICollection<PostImage> PostImages { get; set; } = new List<PostImage>();

    public virtual PostInstagramPublication? PostInstagramPublication { get; set; }

    public virtual PostTelegramPublication? PostTelegramPublication { get; set; }

    public virtual PostXPublication? PostXPublication { get; set; }

    public virtual PostLocation? PostLocation { get; set; }

    public virtual ICollection<PostTranslation> PostTranslations { get; set; } = new List<PostTranslation>();

    public virtual PostType PostType { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
