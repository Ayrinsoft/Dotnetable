namespace Dotnetable.Domain.Entities;

public partial class AdvertisementTranslation
{
    public int AdvertisementTranslationID { get; set; }

    public int AdvertisementID { get; set; }

    public string LanguageCode { get; set; } = null!;

    /// <summary>Localized anchor text. Blank rows are not stored.</summary>
    public string Keyword { get; set; } = null!;

    /// <summary>Optional localized URL; falls back to the advertisement's default <see cref="Advertisement.Url"/>.</summary>
    public string? Url { get; set; }

    public virtual Advertisement Advertisement { get; set; } = null!;
}
