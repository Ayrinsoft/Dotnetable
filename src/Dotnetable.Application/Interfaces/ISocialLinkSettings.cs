using Dotnetable.Application.Social;

namespace Dotnetable.Application.Interfaces;

public interface ISocialLinkSettings
{
    Task<SocialLinkStatus> GetAsync(int websiteId, CancellationToken ct = default);

    Task SaveAsync(int websiteId, SocialLinkInput input, CancellationToken ct = default);
}

public sealed class SocialLinkStatus
{
    public string? WebsiteAddress { get; init; }
    public string? BaseUrl { get; init; }
    public string? PostPath { get; init; }
    public string? PagePath { get; init; }
    public string? ProductPath { get; init; }
    public string? CategoryPath { get; init; }
    public SocialLinkMap Effective { get; init; } = new();
}

public sealed class SocialLinkInput
{
    public string? BaseUrl { get; init; }
    public string? PostPath { get; init; }
    public string? PagePath { get; init; }
    public string? ProductPath { get; init; }
    public string? CategoryPath { get; init; }
}
