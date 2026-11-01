namespace Dotnetable.Domain.Entities;

/// <summary>
/// One line from Google's product taxonomy file. Kept even when the category is not imported,
/// so website 1 can diff a later file and import only the new ids.
/// </summary>
public class GoogleProductTaxonomyEntry
{
    public int GoogleProductTaxonomyEntryID { get; set; }

    public int GoogleId { get; set; }

    public int? ParentGoogleId { get; set; }

    public string Name { get; set; } = null!;

    public string FullPath { get; set; } = null!;

    public byte Depth { get; set; }

    public DateTime LastSeenUtc { get; set; }

    /// <summary>True when the latest fetched file no longer contains this id.</summary>
    public bool AbsentFromLatest { get; set; }
}
