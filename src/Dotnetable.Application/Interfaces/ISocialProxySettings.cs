namespace Dotnetable.Application.Interfaces;

public interface ISocialProxySettings
{
    Task<SocialProxyStatus> GetAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Empty <paramref name="baseUrl"/> turns the proxy off and clears the key.
    /// A blank <paramref name="key"/> keeps the saved key unless <paramref name="clearKey"/> is set.</summary>
    Task SaveAsync(int websiteId, string? baseUrl, string? key, bool clearKey, CancellationToken ct = default);
}

public sealed class SocialProxyStatus
{
    public string? BaseUrl { get; init; }
    public bool HasKey { get; init; }
}
