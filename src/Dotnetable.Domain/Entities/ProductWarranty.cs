namespace Dotnetable.Domain.Entities;

/// <summary>
/// Warranty attached to a product: either a catalog <see cref="Warranty"/> row,
/// free-text custom fields (non-company / one-off), or both (catalog + extra notes).
/// </summary>
public partial class ProductWarranty
{
    public int ProductWarrantyID { get; set; }

    public int ProductID { get; set; }

    /// <summary>When set, title/description/provider resolve from the catalog entry (with localization).</summary>
    public int? WarrantyID { get; set; }

    /// <summary>Free-text title when not using a catalog entry, or override label.</summary>
    public string? CustomTitle { get; set; }

    /// <summary>Free-text body / terms when not using a catalog entry, or extra notes.</summary>
    public string? CustomDescription { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual Product Product { get; set; } = null!;

    public virtual Warranty? Warranty { get; set; }
}
