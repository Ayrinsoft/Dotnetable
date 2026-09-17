using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Page
{
    public int PageID { get; set; }

    public int WebsiteID { get; set; }

    public int? ParentPageID { get; set; }

    public string Slug { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    /// <summary>Overrides the browser-tab/search-result title for this page; falls back to
    /// <see cref="Title"/> plus the site's default when blank.</summary>
    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public string? MetaKeywords { get; set; }

    public string? Template { get; set; }

    public bool IsHomepage { get; set; }

    public byte Status { get; set; }

    public int SortOrder { get; set; }

    /// <summary>Whether visitors may comment on this page. Off by default — most pages
    /// (about, terms, contact) are not discussion pages.</summary>
    public bool CommentsEnabled { get; set; }

    /// <summary>Number of signed-in customers who liked this page (once per customer).</summary>
    public int LikeCount { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual ICollection<ContentComment> ContentComments { get; set; } = new List<ContentComment>();

    public virtual ICollection<Page> InverseParentPage { get; set; } = new List<Page>();

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual ICollection<PageTranslation> PageTranslations { get; set; } = new List<PageTranslation>();

    public virtual Page? ParentPage { get; set; }

    public virtual Website Website { get; set; } = null!;
}
