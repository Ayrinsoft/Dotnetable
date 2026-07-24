using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Reusable warranty definition for a website (company / official plans).
/// Products may reference these or use free-text custom warranties instead.
/// </summary>
public partial class Warranty
{
    public int WarrantyID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Default-language display title (e.g. "18-month official warranty").</summary>
    public string Title { get; set; } = null!;

    /// <summary>Optional longer default-language description / terms summary.</summary>
    public string? Description { get; set; }

    /// <summary>Warranty issuer / company name when applicable (e.g. "Samsung").</summary>
    public string? ProviderName { get; set; }

    /// <summary>Optional structured duration in months for filtering/display.</summary>
    public int? DurationMonths { get; set; }

    public bool IsActive { get; set; }

    public int SortOrder { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual ICollection<ProductWarranty> ProductWarranties { get; set; } = new List<ProductWarranty>();

    public virtual ICollection<WarrantyTranslation> WarrantyTranslations { get; set; } = new List<WarrantyTranslation>();
}
