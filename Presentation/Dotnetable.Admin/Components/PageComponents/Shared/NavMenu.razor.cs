using Blazored.LocalStorage;
using Dotnetable.Admin.Models;
using Microsoft.AspNetCore.Components;

namespace Dotnetable.Admin.Components.PageComponents.Shared;

public partial class NavMenu
{

    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }
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
        themeManager.CurrentCulture = languageCode;
        themeManager.LanguageCode = languageCode.Split('-')[0].ToUpper();
        await _localStorage.SetItemAsync("ThemeManager", themeManager);
        await LanguageChanged.InvokeAsync(languageCode);
        StateHasChanged();
    }

}