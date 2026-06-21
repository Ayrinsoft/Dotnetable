namespace Dotnetable.Domain.Entities;

public class Language
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public bool IsRtl { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Translation> Translations { get; set; } = [];
    public ICollection<WebsiteLanguage> WebsiteLanguages { get; set; } = [];
}
