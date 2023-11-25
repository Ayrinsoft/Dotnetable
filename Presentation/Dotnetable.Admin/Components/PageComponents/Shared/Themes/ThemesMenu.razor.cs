using Dotnetable.Admin.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Shared.Themes;

public partial class ThemesMenu
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }

    [EditorRequired][Parameter] public bool ThemingDrawerOpen { get; set; }
    [EditorRequired][Parameter] public EventCallback<bool> ThemingDrawerOpenChanged { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }
    [EditorRequired][Parameter] public EventCallback<ThemeManagerModel> ThemeManagerChanged { get; set; }
    [EditorRequired][Parameter] public bool RTLLayout { get; set; }


    private readonly List<string> _primaryColors = new()
    {
        Colors.Green.Default,
        Colors.Blue.Default,
        Colors.BlueGrey.Default,
        Colors.Purple.Default,
        Colors.Orange.Default,
        Colors.Red.Default
    };

    private async Task UpdateThemePrimaryColor(string color)
    {
        themeManager.PrimaryColor = color;
        await ThemeManagerChanged.InvokeAsync(themeManager);
    }

    private async Task ToggleDarkLightMode(bool isDarkMode)
    {
        themeManager.IsDarkMode = isDarkMode;
        await ThemeManagerChanged.InvokeAsync(themeManager);
    }
}