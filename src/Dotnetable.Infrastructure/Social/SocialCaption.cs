namespace Dotnetable.Infrastructure.Social;

/// <summary>Puts the public URL on its own line at the end and keeps that URL in one piece.</summary>
public static class SocialCaption
{
    public static string Join(string? body, string? link)
    {
        var text = (body ?? "").Trim();
        var url = (link ?? "").Trim();
        if (url.Length == 0) return text;
        if (text.Length == 0 || EndsWithLink(text, url)) return text.Length == 0 ? url : text;
        return text + "\n" + url;
    }

    /// <summary>One message. The body is shortened so the URL still fits under <paramref name="limit"/>.</summary>
    public static string Fit(string? body, string? link, int limit)
    {
        var joined = Join(body, link);
        if (limit < 1 || joined.Length <= limit) return joined;

        var url = (link ?? "").Trim();
        if (url.Length == 0) return joined[..limit];
        if (url.Length >= limit) return url[..limit];

        var text = StripTrailingLink(body, url);
        var room = limit - url.Length - 1;
        if (room < 1 || text.Length == 0) return url;
        var cut = text.Length <= room ? text : text[..room].TrimEnd();
        return cut.Length == 0 ? url : cut + "\n" + url;
    }

    /// <summary>Several messages. Only the last one ends with the URL, and the URL is never cut in half.</summary>
    public static IReadOnlyList<string> Parts(string? body, string? link, int limit)
    {
        var url = (link ?? "").Trim();
        var text = StripTrailingLink(body, url);
        if (url.Length == 0) return SocialPostPlanner.SplitText(text, limit);
        if (limit < 1) return [];
        if (text.Length == 0) return [url.Length <= limit ? url : url[..limit]];

        var suffix = "\n" + url;
        if (suffix.Length > limit) return [url.Length <= limit ? url : url[..limit]];
        var lastRoom = limit - suffix.Length;
        if (text.Length <= lastRoom) return [text + suffix];

        var parts = new List<string>();
        var rest = text;
        while (rest.Length > lastRoom)
        {
            var take = rest.Length <= limit ? rest.Length - lastRoom : limit;
            if (take < 1) take = 1;
            parts.Add(rest[..take]);
            rest = rest[take..];
        }

        var tail = rest.TrimEnd();
        parts.Add(tail.Length == 0 ? url : tail + suffix);
        return parts;
    }

    private static string StripTrailingLink(string? body, string url)
    {
        var text = (body ?? "").Trim();
        if (url.Length == 0 || !EndsWithLink(text, url)) return text;
        return text[..^url.Length].TrimEnd();
    }

    private static bool EndsWithLink(string text, string url) =>
        text.EndsWith(url, StringComparison.Ordinal);
}
