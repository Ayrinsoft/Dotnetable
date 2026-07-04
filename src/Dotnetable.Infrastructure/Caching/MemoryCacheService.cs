using System.Collections.Concurrent;
using Dotnetable.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Dotnetable.Infrastructure.Caching;

/// <summary>
/// <see cref="ICacheService"/> over <see cref="IMemoryCache"/>. Each tag owns a
/// <see cref="CancellationTokenSource"/> shared by every entry created with that tag; invalidating
/// the tag cancels the token, which evicts all of them, then swaps in a fresh token for future entries.
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _tagTokens = new();

    public MemoryCacheService(IMemoryCache cache) => _cache = cache;

    public async Task<T> GetOrCreateAsync<T>(string key, IEnumerable<string> tags, TimeSpan ttl, Func<Task<T>> factory)
    {
        // TryGetValue reports whether an entry exists at all, not whether its value is non-null —
        // a legitimately cached null (e.g. "no menu at this location") must still count as a hit.
        if (_cache.TryGetValue(key, out T? cached))
            return cached!;

        var value = await factory();

        using var entry = _cache.CreateEntry(key);
        entry.Value = value;
        entry.AbsoluteExpirationRelativeToNow = ttl;
        foreach (var tag in tags)
            entry.AddExpirationToken(new CancellationChangeToken(TokenFor(tag).Token));

        return value;
    }

    public void RemoveByTag(string tag)
    {
        if (_tagTokens.TryRemove(tag, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    private CancellationTokenSource TokenFor(string tag) =>
        _tagTokens.GetOrAdd(tag, _ => new CancellationTokenSource());
}
