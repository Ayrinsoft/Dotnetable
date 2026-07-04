namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Process-local read-through cache with tag-based invalidation. A cached value can carry one or
/// more tags (e.g. the entity type); invalidating a tag drops every entry that carries it,
/// regardless of its individual key.
/// </summary>
public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(string key, IEnumerable<string> tags, TimeSpan ttl, Func<Task<T>> factory);

    /// <summary>Drops every cache entry created with <paramref name="tag"/>.</summary>
    void RemoveByTag(string tag);
}
