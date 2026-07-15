using Dotnetable.Domain.Entities;

namespace Dotnetable.Infrastructure.Data;

/// <summary>
/// Process-wide cache for the (rarely changing) language catalog and each website's active
/// language subset. Mirrors <see cref="TranslationCache"/>: besides avoiding repeat round-trips,
/// it serializes the very first DB fetch behind a lock so concurrent component loads on the same
/// Blazor circuit (which share one scoped <c>AppDbContext</c>) can't both start a query on it at
/// once — that raced and threw "A second operation was started on this context instance...".
/// </summary>
public sealed class LanguageCatalogCache
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private List<Language>? _catalog;
    private readonly Dictionary<int, List<Language>> _perWebsite = new();

    public async Task<List<Language>> GetOrLoadCatalogAsync(Func<Task<List<Language>>> loader)
    {
        if (_catalog is { } cached) return cached;

        await _lock.WaitAsync();
        try
        {
            if (_catalog is { } cachedAfterWait) return cachedAfterWait;
            _catalog = await loader();
            return _catalog;
        }
        finally { _lock.Release(); }
    }

    public async Task<List<Language>> GetOrLoadWebsiteAsync(int websiteId, Func<Task<List<Language>>> loader)
    {
        if (_perWebsite.TryGetValue(websiteId, out var cached)) return cached;

        await _lock.WaitAsync();
        try
        {
            if (_perWebsite.TryGetValue(websiteId, out var cachedAfterWait)) return cachedAfterWait;
            var loaded = await loader();
            _perWebsite[websiteId] = loaded;
            return loaded;
        }
        finally { _lock.Release(); }
    }

    /// <summary>Drops every cached entry — call after any write so the next read re-fetches.</summary>
    public void InvalidateAll()
    {
        _catalog = null;
        _perWebsite.Clear();
    }
}
