namespace Dotnetable.Infrastructure.Social;

/// <summary>Puts the public URL on its own line at the end and keeps that URL in one piece.</summary>
public static class SocialCaption
{
    public static string Join(string? body, string? link, string? hashtags = null)
    {
        var text = (body ?? "").Trim();
        var tail = Tail(link, hashtags);
        if (tail.Length == 0) return text;
        if (text.Length == 0 || text.EndsWith(tail, StringComparison.Ordinal))
            return text.Length == 0 ? tail : text;
        return text + "\n" + tail;
    }

    /// <summary>One message. The body is shortened so the link and hashtags still fit under <paramref name="limit"/>.</summary>
    public static string Fit(string? body, string? link, int limit, string? hashtags = null)
    {
        var text = StripTrailingTail(body, link, hashtags);
        var tail = Tail(link, hashtags);
        if (tail.Length == 0)
        {
            if (limit < 1 || text.Length <= limit) return text;
            return text[..limit];
        }

        var joined = text.Length == 0 ? tail : text + "\n" + tail;
        if (limit < 1 || joined.Length <= limit) return joined;
        if (tail.Length >= limit) return PreferLink(link, tail, limit);

        var room = limit - tail.Length - 1;
        if (room < 1 || text.Length == 0) return tail;
        var cut = text.Length <= room ? text : text[..room].TrimEnd();
        return cut.Length == 0 ? tail : cut + "\n" + tail;
    }

    /// <summary>Several messages. Only the last one ends with the link and hashtags, and that ending is never cut in half.</summary>
    public static IReadOnlyList<string> Parts(string? body, string? link, int limit, string? hashtags = null)
    {
        var text = StripTrailingTail(body, link, hashtags);
        var tail = Tail(link, hashtags);
        if (tail.Length == 0) return SocialPostPlanner.SplitText(text, limit);
        if (limit < 1) return [];
        if (text.Length == 0) return [tail.Length <= limit ? tail : PreferLink(link, tail, limit)];

        var suffix = "\n" + tail;
        if (suffix.Length > limit) return [tail.Length <= limit ? tail : PreferLink(link, tail, limit)];
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

        var end = rest.TrimEnd();
        parts.Add(end.Length == 0 ? tail : end + suffix);
        return parts;
    }

    /// <summary>The public link, then the hashtag line. The hashtags are the last line of the message.</summary>
    private static string Tail(string? link, string? hashtags)
    {
        var url = (link ?? "").Trim();
        var tags = (hashtags ?? "").Trim();
        if (url.Length == 0) return tags;
        if (tags.Length == 0) return url;
        return url + "\n" + tags;
    }

    private static string PreferLink(string? link, string tail, int limit)
    {
        var url = (link ?? "").Trim();
        if (url.Length > 0 && url.Length <= limit) return url;
        return tail[..limit];
    }

    private static string StripTrailingTail(string? body, string? link, string? hashtags)
    {
        var text = (body ?? "").Trim();
        var tail = Tail(link, hashtags);
        if (tail.Length == 0 || !text.EndsWith(tail, StringComparison.Ordinal)) return text;
        return text[..^tail.Length].TrimEnd();
    }
}
