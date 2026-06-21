namespace Dotnetable.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public int WebsiteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
    public int LanguageId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Website? Website { get; set; }
    public Language? Language { get; set; }
}
