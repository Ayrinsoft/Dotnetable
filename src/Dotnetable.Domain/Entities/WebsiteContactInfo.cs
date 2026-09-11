using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>An admin-defined contact detail (phone, email, address, working hours, ...), free-form
/// title/value pair grouped under an admin-chosen category (e.g. "Sales Office", "Factory").</summary>
public partial class WebsiteContactInfo
{
    public int WebsiteContactInfoID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Admin-defined group name (e.g. "Sales Office", "Factory"), used to cluster related
    /// rows on the public page. Optional — ungrouped rows render under a generic heading.</summary>
    public string? GroupTitle { get; set; }

    public string Title { get; set; } = null!;

    public string Value { get; set; } = null!;

    public string? Icon { get; set; }

    public int SortOrder { get; set; }

    public virtual Website Website { get; set; } = null!;
}
