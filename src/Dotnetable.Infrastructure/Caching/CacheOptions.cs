namespace Dotnetable.Infrastructure.Caching;

/// <summary>
/// Default TTL for the cached decorators (Menu/Category/Page/Post). Acts as a safety-net expiry —
/// the tag invalidation on write is what keeps entries fresh; the TTL just bounds staleness if a
/// write path is ever missed.
/// </summary>
public class CacheOptions
{
    public const string SectionName = "Cache";

    public int DefaultTtlMinutes { get; set; } = 10;

    public TimeSpan DefaultTtl => TimeSpan.FromMinutes(DefaultTtlMinutes);
}
