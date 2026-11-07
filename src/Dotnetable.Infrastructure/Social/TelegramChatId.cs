using Dotnetable.Application.Instagram;

namespace Dotnetable.Infrastructure.Social;

/// <summary>Accepts @name, a numeric id, or a public t.me link.</summary>
public static class TelegramChatId
{
    public static string Normalize(string? raw)
    {
        var text = (raw ?? "").Trim();
        if (text.Length == 0)
            throw new SocialPublishException("Enter the Telegram chat id, such as @channel.");

        if ((text[0] == '-' || char.IsDigit(text[0])) && long.TryParse(text, out _))
            return text;

        text = FromLink(text).Trim().TrimStart('@');
        if (text.Length is < 5 or > 32 || text.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '_'))
            throw new SocialPublishException("Enter the public channel as @name or t.me/name, or the numeric id that starts with -100.");
        return "@" + text;
    }

    private static string FromLink(string text)
    {
        var candidate = text.Contains("://", StringComparison.Ordinal) ? text : "https://" + text;
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
            return text;
        if (!uri.Host.Equals("t.me", StringComparison.OrdinalIgnoreCase)
            && !uri.Host.Equals("telegram.me", StringComparison.OrdinalIgnoreCase)
            && !uri.Host.Equals("telegram.dog", StringComparison.OrdinalIgnoreCase))
            return text;

        var path = uri.AbsolutePath.Trim('/');
        if (path.Length == 0 || path.StartsWith('+') || path.StartsWith("joinchat/", StringComparison.OrdinalIgnoreCase) || path.StartsWith("c/", StringComparison.OrdinalIgnoreCase))
            throw new SocialPublishException("An invite link cannot be checked. Use the public @name or the numeric id that starts with -100.");

        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var name = parts[0].Equals("s", StringComparison.OrdinalIgnoreCase) && parts.Length > 1 ? parts[1] : parts[0];
        return name;
    }
}
