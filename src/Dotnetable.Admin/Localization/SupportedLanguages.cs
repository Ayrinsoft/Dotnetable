using Dotnetable.Domain.Entities;

namespace Dotnetable.Admin.Localization;

public sealed record LanguageOption(string Code, string Native, string Flag, bool Rtl);

/// <summary>Cookie name shared by the header switcher, the auth pages, and the built-in defaults
/// used to seed the master language catalog (see <see cref="ILanguageService"/>) and by the Setup
/// wizard, which runs before any website — and therefore no catalog row — exists in the DB.</summary>
public static class SupportedLanguages
{
    public const string CookieName = "dn-lang";

    public static readonly LanguageOption[] Defaults =
    [
        new("en", "English",  "🇬🇧", false),
        new("de", "Deutsch",  "🇩🇪", false),
        new("fr", "Français", "🇫🇷", false),
        new("ru", "Русский",  "🇷🇺", false),
        new("zh", "中文",      "🇨🇳", false),
        new("fa", "فارسی",    "🇮🇷", true),
        new("ar", "العربية",  "🇸🇦", true),
    ];

    private static readonly Dictionary<string, string> Flags = Defaults.ToDictionary(l => l.Code, l => l.Flag);

    /// <summary>Maps a DB <see cref="Language"/> catalog row to the UI-facing option shape, reusing
    /// a known flag emoji for the built-in codes and falling back to a globe for new ones.</summary>
    public static LanguageOption ToOption(this Language language) =>
        new(language.LanguageCode, language.Name, Flags.GetValueOrDefault(language.LanguageCode, "🌐"), language.RTLDesign);

    public static bool IsSupported(this IReadOnlyCollection<LanguageOption> options, string? code) =>
        !string.IsNullOrEmpty(code) && options.Any(l => l.Code == code);

    public static LanguageOption Get(this IReadOnlyCollection<LanguageOption> options, string? code) =>
        options.FirstOrDefault(l => l.Code == code) ?? options.FirstOrDefault() ?? Defaults[0];

    public static bool IsRtl(this IReadOnlyCollection<LanguageOption> options, string? code) =>
        options.Get(code).Rtl;
}
