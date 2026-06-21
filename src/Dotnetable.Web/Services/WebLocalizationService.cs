using System.Collections.Concurrent;

namespace Dotnetable.Web.Services;

public class WebLocalizationService
{
    private ConcurrentDictionary<string, string> _translations = new();

    public async Task LoadFromApiAsync(ApiClient client, int languageId, CancellationToken ct = default)
    {
        var data = await client.GetTranslationsAsync(languageId, ct);
        _translations = new ConcurrentDictionary<string, string>(data);
    }

    public string Get(string key, string? fallback = null) =>
        _translations.TryGetValue(key, out var value) ? value : fallback ?? key;
}
