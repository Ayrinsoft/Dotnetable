using MudBlazor;

namespace Dotnetable.Admin.Theme;

/// <summary>
/// Browser-local admin color skins (MudBlazor palettes). Choice is stored in localStorage only —
/// never synced to the member or database.
/// </summary>
public sealed record AdminSkin(string Id, string Name, string Primary, string Secondary, string Drawer);

public static class AdminSkinCatalog
{
    public const string DefaultId = "blue";
    public const string StorageKey = "dn-admin-skin";

    public static IReadOnlyList<AdminSkin> All { get; } =
    [
        new("blue",   "Blue",   "#348fe2", "#727cb6", "#2d353c"),
        new("teal",   "Teal",   "#00acac", "#49b6d6", "#1e3a3a"),
        new("purple", "Purple", "#727cb6", "#9b6b9e", "#2a2438"),
        new("green",  "Green",  "#32a852", "#6bbf59", "#1f2e24"),
        new("orange", "Orange", "#f59c1a", "#e87e04", "#3a2e1f"),
        new("rose",   "Rose",   "#e83e8c", "#ff5b57", "#3a1f2a"),
    ];

    public static AdminSkin Get(string? id) =>
        All.FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase))
        ?? All[0];

    public static MudTheme BuildMudTheme(AdminSkin skin) => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary             = skin.Primary,
            PrimaryContrastText = "#FFFFFF",
            Secondary           = skin.Secondary,
            Tertiary            = skin.Secondary,
            Info                = "#49b6d6",
            Success             = "#00acac",
            Warning             = "#f59c1a",
            Error               = "#ff5b57",
            Dark                = "#2d353c",
            Background          = "#e4e7ea",
            Surface             = "#FFFFFF",
            TextPrimary         = "#2d353c",
            TextSecondary       = "#7b8488",
            AppbarBackground    = "#FFFFFF",
            AppbarText          = "#2d353c",
            DrawerBackground    = skin.Drawer,
            DrawerText          = "#a8acb1",
            DrawerIcon          = "#a8acb1",
            LinesDefault        = "#e2e7eb",
            TableLines          = "#e2e7eb",
        },
        PaletteDark = new PaletteDark
        {
            Primary             = skin.Primary,
            PrimaryContrastText = "#FFFFFF",
            Secondary           = skin.Secondary,
            Tertiary            = skin.Secondary,
            Info                = "#49b6d6",
            Success             = "#00acac",
            Warning             = "#f59c1a",
            Error               = "#ff5b57",
            Dark                = "#1a2025",
            Background          = "#1a2025",
            BackgroundGray      = "#151a1e",
            Surface             = "#2d353c",
            TextPrimary         = "#cdd3d8",
            TextSecondary       = "#8a939a",
            AppbarBackground    = "#2d353c",
            AppbarText          = "#cdd3d8",
            DrawerBackground    = DarkenDrawer(skin.Drawer),
            DrawerText          = "#a8acb1",
            DrawerIcon          = "#a8acb1",
            LinesDefault        = "#3a4148",
            TableLines          = "#3a4148",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "4px",
            DrawerWidthLeft     = "240px",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = new[] { "Open Sans", "sans-serif" } }
        }
    };

    private static string DarkenDrawer(string hex)
    {
        // Slightly darker drawer for dark mode while keeping the skin identity.
        if (hex.Length != 7 || hex[0] != '#') return "#1b2127";
        static int Channel(string h, int i) => Convert.ToInt32(h.Substring(i, 2), 16);
        var r = Math.Max(0, Channel(hex, 1) - 18);
        var g = Math.Max(0, Channel(hex, 3) - 18);
        var b = Math.Max(0, Channel(hex, 5) - 18);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}
