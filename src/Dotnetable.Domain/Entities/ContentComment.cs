using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// A visitor comment on a blog <see cref="Post"/> or a CMS <see cref="Page"/> — exactly one of
/// <see cref="PostID"/>/<see cref="PageID"/> is set. Submitted comments stay hidden until a moderator
/// approves them; replies written from the admin panel carry <see cref="AuthorMemberID"/> and are
/// published immediately. Product feedback lives in <see cref="ProductReview"/>, not here.
/// </summary>
public partial class ContentComment
{
    public int ContentCommentID { get; set; }

    public int WebsiteID { get; set; }

    public int? PostID { get; set; }

    public int? PageID { get; set; }

    /// <summary>The comment this one replies to (same post/page), or null for a top-level comment.</summary>
    public int? ParentCommentID { get; set; }

    /// <summary>Signed-in customer who wrote it; null for a guest or an admin reply.</summary>
    public int? WebsiteClientID { get; set; }

    /// <summary>Admin member who wrote it (a reply from the panel); null for visitor comments.</summary>
    public int? AuthorMemberID { get; set; }

    public string AuthorName { get; set; } = null!;

    /// <summary>Only ever shown in the admin panel — never returned by the public API.</summary>
    public string? AuthorEmail { get; set; }

    public string Body { get; set; } = null!;

    /// <summary>Values of <c>ModerationStatus</c> (1 pending, 2 approved, 3 rejected).</summary>
    public byte Status { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModeratedAt { get; set; }

    public int? ModeratedByMemberID { get; set; }

    public virtual Member? AuthorMember { get; set; }

    public virtual ICollection<ContentComment> InverseParentComment { get; set; } = new List<ContentComment>();

    public virtual Member? ModeratedByMember { get; set; }

    public virtual Page? Page { get; set; }

    public virtual ContentComment? ParentComment { get; set; }

    public virtual Post? Post { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient? WebsiteClient { get; set; }
}
