namespace Dotnetable.Application.Interfaces;

public interface IShortLinkService
{
    Task<IReadOnlyList<ShortLinkRow>> ListAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Returns the public <c>/l/{code}</c> address, creating the row when this target is new.</summary>
    Task<string> ToPublicAsync(int websiteId, string targetUrl, CancellationToken ct = default);

    Task<ShortLinkRow> CreateAsync(int websiteId, string targetUrl, CancellationToken ct = default);

    Task UpdateTargetAsync(int websiteId, int shortLinkId, string targetUrl, CancellationToken ct = default);

    Task DeleteAsync(int websiteId, int shortLinkId, CancellationToken ct = default);

    /// <summary>The stored address for this website and code, or null.</summary>
    Task<string?> ResolveAsync(int websiteId, string code, CancellationToken ct = default);
}

public sealed class ShortLinkRow
{
    public int Id { get; init; }
    public string Code { get; init; } = "";
    public string PublicUrl { get; init; } = "";
    public string TargetUrl { get; init; } = "";
    public DateTime CreatedAt { get; init; }
}
