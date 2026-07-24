namespace Dotnetable.Domain.Entities;

public partial class WarrantyTranslation
{
    public int WarrantyTranslationID { get; set; }

    public int WarrantyID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? ProviderName { get; set; }

    public virtual Warranty Warranty { get; set; } = null!;
}
