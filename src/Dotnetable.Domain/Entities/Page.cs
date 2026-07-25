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

    public string? Template { get; set; }

    public bool IsHomepage { get; set; }

    public byte Status { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual ICollection<Page> InverseParentPage { get; set; } = new List<Page>();

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual ICollection<PageTranslation> PageTranslations { get; set; } = new List<PageTranslation>();

    public virtual Page? ParentPage { get; set; }

    public virtual Website Website { get; set; } = null!;
}
