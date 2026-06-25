namespace Dotnetable.Application;

/// <summary>
/// Single source of truth for the product/brand name shown in page titles and chrome
/// (e.g. "Members — Dotnetable"). Defaults to "Dotnetable"; override once at startup from the
/// configuration key <c>Branding:AppName</c>. Change it here (or in config) and every screen follows.
/// </summary>
public static class AppBranding
{
    public static string Name { get; set; } = "Dotnetable";
}
