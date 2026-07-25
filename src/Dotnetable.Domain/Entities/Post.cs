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

    public int? FeaturedImageFileID { get; set; }

    public byte Status { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public bool IsFeatured { get; set; }

    public int ViewCount { get; set; }

    public bool CommentsEnabled { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Member? AuthorMember { get; set; }

    public virtual FileRecord? FeaturedImageFile { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();

    public virtual ICollection<PostTranslation> PostTranslations { get; set; } = new List<PostTranslation>();

    public virtual PostType PostType { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
