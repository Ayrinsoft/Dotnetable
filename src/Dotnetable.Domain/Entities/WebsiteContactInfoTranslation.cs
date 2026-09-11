namespace Dotnetable.Domain.Entities;

public partial class WebsiteContactInfoTranslation
{
    public int WebsiteContactInfoTranslationID { get; set; }

    public int WebsiteContactInfoID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string? GroupTitle { get; set; }

    /// <summary>Localized display name. Blank rows are not stored.</summary>
    public string Title { get; set; } = null!;

    /// <summary>Optional localized value; falls back to <see cref="WebsiteContactInfo.Value"/> when blank
    /// (e.g. a phone number usually doesn't need translating, but working hours text does).</summary>
    public string? Value { get; set; }

    public virtual WebsiteContactInfo WebsiteContactInfo { get; set; } = null!;
}
