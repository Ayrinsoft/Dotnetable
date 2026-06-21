using System.Collections.Concurrent;

namespace Dotnetable.Infrastructure.Data;

public class TranslationCache
{
    private readonly ConcurrentDictionary<string, string> _entries = new();

    public void Set(int websiteId, int languageId, string key, string value) =>
        _entries[BuildKey(websiteId, languageId, key)] = value;

    public bool TryGet(int websiteId, int languageId, string key, out string value)
    {
        if (_entries.TryGetValue(BuildKey(websiteId, languageId, key), out var v)) { value = v; return true; }
        if (_entries.TryGetValue(BuildKey(0, languageId, key), out v)) { value = v; return true; }
        value = string.Empty;
        return false;
    }

    public void Load(IEnumerable<(int websiteId, int languageId, string key, string value)> entries)
    {
        foreach (var (wId, lId, k, v) in entries)
            _entries[BuildKey(wId, lId, k)] = v;
    }

    private static string BuildKey(int websiteId, int languageId, string key) =>
        $"{websiteId}:{languageId}:{key}";
}
