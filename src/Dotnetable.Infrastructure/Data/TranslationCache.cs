using System.Collections.Concurrent;

namespace Dotnetable.Infrastructure.Data;

public class TranslationCache
{
    private readonly ConcurrentDictionary<string, string> _entries = new();

    public void Set(int websiteId, string languageCode, string key, string value) =>
        _entries[BuildKey(websiteId, languageCode, key)] = value;

    public bool TryGet(int websiteId, string languageCode, string key, out string value)
    {
        if (_entries.TryGetValue(BuildKey(websiteId, languageCode, key), out var v)) { value = v; return true; }
        if (_entries.TryGetValue(BuildKey(0, languageCode, key), out v)) { value = v; return true; }
        value = string.Empty;
        return false;
    }

    public void Load(IEnumerable<(int websiteId, string languageCode, string key, string value)> entries)
    {
        foreach (var (wId, lc, k, v) in entries)
            _entries[BuildKey(wId, lc, k)] = v;
    }

    private static string BuildKey(int websiteId, string languageCode, string key) =>
        $"{websiteId}:{languageCode}:{key}";
}
