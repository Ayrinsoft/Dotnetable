namespace Dotnetable.Domain.Entities;

public class ApiKey
{
    public int Id { get; set; }
    public int WebsiteId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AllowedIps { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }

    public Website? Website { get; set; }

    public IEnumerable<string> GetAllowedIpList() =>
        string.IsNullOrWhiteSpace(AllowedIps)
            ? []
            : AllowedIps.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
