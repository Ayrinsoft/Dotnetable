namespace Dotnetable.Admin.Components.Pages.Content;

/// <summary>The languages offered for per-record content translations across the admin content pages.</summary>
public static class ContentLanguages
{
    public static readonly (string Code, string Label)[] All =
    [
        ("en", "English"),
        ("de", "Deutsch"),
        ("fr", "Français"),
        ("ru", "Русский"),
        ("zh", "中文"),
        ("fa", "فارسی"),
        ("ar", "العربية"),
    ];
}
