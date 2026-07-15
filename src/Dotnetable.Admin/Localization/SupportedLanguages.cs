namespace Dotnetable.Admin.Localization;

public sealed record LanguageOption(string Code, string Native, string Flag, bool Rtl);

/// <summary>Single source of truth for the language list shown in the Setup wizard, the
/// pre-login auth pages, and the admin header switcher.</summary>
public static class SupportedLanguages
{
    public const string CookieName = "dn-lang";

    public static readonly LanguageOption[] All =
    [
        new("en", "English",  "🇬🇧", false),
        new("de", "Deutsch",  "🇩🇪", false),
        new("fr", "Français", "🇫🇷", false),
        new("ru", "Русский",  "🇷🇺", false),
        new("zh", "中文",      "🇨🇳", false),
        new("fa", "فارسی",    "🇮🇷", true),
        new("ar", "العربية",  "🇸🇦", true),
    ];

    public static bool IsSupported(string? code) =>
        !string.IsNullOrEmpty(code) && All.Any(l => l.Code == code);

    public static LanguageOption Get(string? code) =>
        All.FirstOrDefault(l => l.Code == code) ?? All[0];

    public static bool IsRtl(string? code) => Get(code).Rtl;
}
