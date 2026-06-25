using System.Collections.Concurrent;

namespace Dotnetable.Infrastructure.Data;

/// <summary>
/// Thread-safe buffer of localization keys discovered at render time that are not yet in the
/// database. Front-end localizers push (websiteId, key, default) here on a cache miss; a background
/// service drains it and inserts the missing <c>LocalizationKey</c> rows so an admin can translate
/// them later. Buffering keeps page rendering free of database writes.
/// </summary>
public sealed class PendingTranslationKeys
{
    private readonly ConcurrentDictionary<(int WebsiteId, string Key), string> _pending = new();

    public void Add(int websiteId, string key, string defaultValue)
    {
        if (websiteId <= 0 || string.IsNullOrWhiteSpace(key)) return;
        _pending.TryAdd((websiteId, key), defaultValue ?? key);
    }

    public bool IsEmpty => _pending.IsEmpty;

    /// <summary>Atomically removes and returns everything buffered so far.</summary>
    public IReadOnlyList<(int WebsiteId, string Key, string DefaultValue)> Drain()
    {
        var taken = new List<(int, string, string)>();
        foreach (var entry in _pending)
            if (_pending.TryRemove(entry.Key, out var def))
                taken.Add((entry.Key.WebsiteId, entry.Key.Key, def));
        return taken;
    }
}
