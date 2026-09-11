using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>An admin-defined contact detail (phone, email, address, working hours, ...): a fixed
/// <see cref="ContactType"/> (so the admin picks from a known list instead of free-typing a kind)
/// plus a free-form title/value, grouped under an admin-chosen category (e.g. "Sales Office").</summary>
public partial class WebsiteContactInfo
{
    public int WebsiteContactInfoID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>One of <see cref="Enums.ContactInfoType"/>. Drives the default label/icon shown in
    /// the admin picker so phone/email/address/location/hours rows are unambiguous at a glance.</summary>
    public byte ContactType { get; set; }

    /// <summary>Admin-defined group name (e.g. "Sales Office", "Factory"), used to cluster related
    /// rows on the public page. Optional — ungrouped rows render under a generic heading.</summary>
    public string? GroupTitle { get; set; }

    public string Title { get; set; } = null!;

    public string Value { get; set; } = null!;

    public string? Icon { get; set; }

    public int SortOrder { get; set; }

    public virtual Website Website { get; set; } = null!;
}
