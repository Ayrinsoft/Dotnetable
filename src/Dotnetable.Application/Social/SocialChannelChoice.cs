using Dotnetable.Application.Instagram;

namespace Dotnetable.Application.Social;

/// <summary>Shared rules for naming a social channel and picking one when several exist.</summary>
public static class SocialChannelChoice
{
    public static (string Title, string? LanguageCode) Read(string? title, string? languageCode)
    {
        var name = (title ?? "").Trim();
        if (name.Length == 0)
            throw new SocialPublishException("Enter a name for this channel.");
        if (name.Length > 80)
            throw new SocialPublishException("The channel name must be 80 characters or fewer.");
        var language = string.IsNullOrWhiteSpace(languageCode) ? null : languageCode.Trim();
        if (language is { Length: > 10 })
            throw new SocialPublishException("The language code must be 10 characters or fewer.");
        return (name, language);
    }

    /// <summary>One matching language wins. Otherwise the first channel in the list.</summary>
    public static int Prefer(IReadOnlyList<(int Id, string? LanguageCode)> accounts, string? languageCode)
    {
        if (accounts.Count == 0) return 0;
        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            var matches = accounts
                .Where(account => string.Equals(account.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (matches.Count == 1) return matches[0].Id;
        }

        return accounts[0].Id;
    }

    public static string Format(string? title, string? handle, string? languageCode)
    {
        var name = (title ?? "").Trim();
        var who = (handle ?? "").Trim().TrimStart('@');
        if (name.Length == 0) name = who;
        if (name.Length == 0) name = "—";
        if (!string.IsNullOrWhiteSpace(languageCode))
            name += " · " + languageCode.Trim();
        if (who.Length > 0 && !string.IsNullOrWhiteSpace(title) && !name.Contains(who, StringComparison.OrdinalIgnoreCase))
            name += " (@" + who + ")";
        return name;
    }
}
