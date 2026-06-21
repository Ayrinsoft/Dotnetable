namespace Dotnetable.Domain.Entities;

public class Translation
{
    public int Id { get; set; }
    public int LanguageId { get; set; }
    public int? WebsiteId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Language? Language { get; set; }
    public Website? Website { get; set; }
}
