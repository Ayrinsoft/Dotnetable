namespace Dotnetable.Domain.Entities;

public class WebsiteLanguage
{
    public int WebsiteId { get; set; }
    public int LanguageId { get; set; }
    public bool IsDefault { get; set; }

    public Website? Website { get; set; }
    public Language? Language { get; set; }
}
