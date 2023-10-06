using Blazored.LocalStorage;
using Dotnetable.Admin.Models;
using Microsoft.AspNetCore.Components;

namespace Dotnetable.Admin.Components.Shared;

public partial class NavMenu
{

    [EditorRequired][Parameter] public ThemeManagerModel ThemeManager { get; set; }
    [EditorRequired][Parameter] public bool CanMiniSideMenuDrawer { get; set; }
    [EditorRequired][Parameter] public EventCallback ToggleSideMenuDrawer { get; set; }
    [EditorRequired][Parameter] public EventCallback OpenCommandPalette { get; set; }
    [EditorRequired][Parameter] public UserModel User { get; set; }
    [EditorRequired][Parameter] public EventCallback<string> LanguageChanged { get; set; }
    [Inject] private ILocalStorageService _localStorage { get; set; }


    private string _languageCode = "";
    private async Task ChangeLanguageCode(string languageCode)
    {
        if (_languageCode == languageCode) return;
        _languageCode = languageCode;
        ThemeManager.CurrentCulture = languageCode;
        ThemeManager.LanguageCode = languageCode.Split('-')[0].ToUpper();
        await _localStorage.SetItemAsync("ThemeManager", ThemeManager);
        await LanguageChanged.InvokeAsync(languageCode);
        StateHasChanged();
    }

}