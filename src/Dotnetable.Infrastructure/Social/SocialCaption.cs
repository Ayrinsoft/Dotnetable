using System.Net;

namespace Dotnetable.Infrastructure.Social;

/// <summary>
/// Puts the public URL at the end, with a blank line before it and a blank line after it
/// when hashtags follow. The URL and the hashtag line stay in one piece.
/// </summary>
public static class SocialCaption
{
    public static string Join(string? body, string? link, string? hashtags = null)
    {
        var text = StripTrailingTail(body, link, hashtags);
        var block = Block(link, hashtags);
        if (block.Length == 0) return text;
        if (text.Length == 0) return block.TrimStart('\r', '\n');
        return text + block;
    }

    /// <summary>One message. The body is shortened so the link and hashtags still fit under <paramref name="limit"/>.</summary>
    public static string Fit(string? body, string? link, int limit, string? hashtags = null)
    {
        var text = StripTrailingTail(body, link, hashtags);
        var block = Block(link, hashtags);
        if (block.Length == 0)
        {
            if (limit < 1 || text.Length <= limit) return text;
            return text[..limit];
        }

        var joined = text.Length == 0 ? block.TrimStart('\r', '\n') : text + block;
        if (limit < 1 || joined.Length <= limit) return joined;
        if (text.Length == 0 || block.Length >= limit) return PreferLink(link, block.TrimStart('\r', '\n'), limit);

        var room = limit - block.Length;
        if (room < 1) return PreferLink(link, block.TrimStart('\r', '\n'), limit);
        var cut = text.Length <= room ? text : text[..room].TrimEnd();
        return cut.Length == 0 ? block.TrimStart('\r', '\n') : cut + block;
    }

    /// <summary>Several messages. Only the last one ends with the link and hashtags, and that ending is never cut in half.</summary>
    public static IReadOnlyList<string> Parts(string? body, string? link, int limit, string? hashtags = null)
    {
        var text = StripTrailingTail(body, link, hashtags);
        var block = Block(link, hashtags);
        if (block.Length == 0) return SocialPostPlanner.SplitText(text, limit);
        if (limit < 1) return [];
        var ending = block.TrimStart('\r', '\n');
        if (text.Length == 0) return [ending.Length <= limit ? ending : PreferLink(link, ending, limit)];
        if (block.Length >= limit) return [ending.Length <= limit ? ending : PreferLink(link, ending, limit)];

        var lastRoom = limit - block.Length;
        if (text.Length <= lastRoom) return [text + block];

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
        parts.Add(end.Length == 0 ? ending : end + block);
        return parts;
    }

    /// <summary>A blank line, the link, then a blank line and the hashtags.</summary>
    private static string Block(string? link, string? hashtags)
    {
        var url = (link ?? "").Trim();
        var tags = (hashtags ?? "").Trim();
        if (url.Length == 0 && tags.Length == 0) return "";
        if (url.Length == 0) return "\n\n" + tags;
        if (tags.Length == 0) return "\n\n" + url;
        return "\n\n" + url + "\n\n" + tags;
    }

    private static string PreferLink(string? link, string tail, int limit)
    {
        var url = (link ?? "").Trim();
        if (url.Length > 0 && url.Length <= limit) return url;
        if (limit < 1 || tail.Length <= limit) return tail;
        return tail[..limit];
    }

    /// <summary>
    /// Telegram HTML. The public address becomes «برای اطلاعات بیشتر اینجا کلیک کنید» and the word اینجا is the link.
    /// The rest of the text is escaped so a caption cannot break the message.
    /// </summary>
    public static string TelegramHtml(string? plain, string? link)
    {
        var html = WebUtility.HtmlEncode(plain ?? "");
        var url = (link ?? "").Trim();
        if (url.Length == 0) return html;
        var encodedUrl = WebUtility.HtmlEncode(url);
        var sentence = "برای اطلاعات بیشتر <a href=\"" + encodedUrl + "\">اینجا</a> کلیک کنید";
        if (html.Contains(encodedUrl, StringComparison.Ordinal))
            return html.Replace(encodedUrl, sentence, StringComparison.Ordinal);
        return html.Length == 0 ? sentence : html + "\n\n" + sentence;
    }

    private static string StripTrailingTail(string? body, string? link, string? hashtags)
    {
        var text = (body ?? "").Trim();
        var tags = (hashtags ?? "").Trim();
        var url = (link ?? "").Trim();
        if (tags.Length > 0 && text.EndsWith(tags, StringComparison.Ordinal))
            text = text[..^tags.Length].TrimEnd();
        if (url.Length > 0 && text.EndsWith(url, StringComparison.Ordinal))
            text = text[..^url.Length].TrimEnd();
        return text;
    }
}
