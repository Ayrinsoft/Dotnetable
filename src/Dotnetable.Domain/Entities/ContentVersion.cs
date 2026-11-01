namespace Dotnetable.Domain.Entities;

/// <summary>
/// A website's content release line (for example <c>1.2</c>) used by versioned posts.
/// This is not an API contract version and not a commerce product version.
/// </summary>
public class ContentVersion
{
    public int ContentVersionID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Release label such as <c>1.2</c>. Unique per website.</summary>
    public string Name { get; set; } = null!;

    public int SortOrder { get; set; }

    /// <summary>The release used when a reader does not ask for a version. One per website.</summary>
    public bool IsLatest { get; set; }

    /// <summary>Hidden from the public version list until this is on.</summary>
    public bool IsPublished { get; set; }

    public virtual Website Website { get; set; } = null!;
}
