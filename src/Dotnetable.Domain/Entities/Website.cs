namespace Dotnetable.Domain.Entities;

public class Website
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int DefaultLanguageId { get; set; }
    public string? ActiveTheme { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Language? DefaultLanguage { get; set; }
    public ICollection<Post> Posts { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
    public ICollection<ApiKey> ApiKeys { get; set; } = [];
    public ICollection<WebsiteLanguage> WebsiteLanguages { get; set; } = [];
}
