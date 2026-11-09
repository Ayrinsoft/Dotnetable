namespace Dotnetable.Application.Social;

/// <summary>
/// Post keywords are stored as a comma-separated list. A selected keyword becomes one hashtag:
/// spaces turn into underscores, so <c>آیتم 1</c> is <c>#آیتم_1</c>.
/// </summary>
public static class SocialHashtags
{
    public static IReadOnlyList<string> Split(string? stored)
    {
        if (string.IsNullOrWhiteSpace(stored)) return [];
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var list = new List<string>();
        foreach (var part in stored.Split([',', '،', '٬', ';', '؛'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (part.Length == 0 || !seen.Add(part)) continue;
            list.Add(part);
        }

        return list;
    }

    /// <summary>One line of hashtags in the given order. Empty when nothing can become a tag.</summary>
    public static string? Line(IEnumerable<string>? keywords)
    {
        if (keywords is null) return null;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var tags = new List<string>();
        foreach (var keyword in keywords)
        {
            var tag = Format(keyword);
            if (tag is null || !seen.Add(tag)) continue;
            tags.Add(tag);
            if (tags.Count == 30) break;
        }

        return tags.Count == 0 ? null : string.Join(' ', tags);
    }

    public static string? Format(string? keyword)
    {
        var text = keyword?.Trim().TrimStart('#').Trim();
        if (string.IsNullOrEmpty(text)) return null;

        var buffer = new char[text.Length];
        var length = 0;
        var underscore = false;
        foreach (var ch in text)
        {
            if (char.IsWhiteSpace(ch) || ch is '_' or '-' or '–' or '—')
            {
                if (length > 0 && !underscore)
                {
                    buffer[length++] = '_';
                    underscore = true;
                }

                continue;
            }

            if (ch is '#' or ',' or '،' or '.' or '!' or '?' or '/' or '\\' or ':' or ';' or '"' or '\'' or '(' or ')' or '[' or ']' or '|' or '&' or '%' or '+' or '=' or '@' or '*' or '~')
                continue;

            buffer[length++] = ch;
            underscore = false;
        }

        while (length > 0 && buffer[length - 1] == '_') length--;
        return length == 0 ? null : "#" + new string(buffer, 0, length);
    }
}
