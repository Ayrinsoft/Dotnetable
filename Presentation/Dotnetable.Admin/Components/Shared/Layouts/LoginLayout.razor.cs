using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Dotnetable.Admin.Models;
using System.Globalization;
using Blazored.LocalStorage;

namespace Dotnetable.Admin.Components.Shared.Layouts;

public partial class LoginLayout
{
    [CascadingParameter] private Task<AuthenticationState> _authenticationStateTask { get; set; }
    [Inject] private NavigationManager _navigationManager { get; set; }
    [Inject] private ILocalStorageService _localStorage { get; set; }

    private ThemeManagerModel _themeManagerModel { get; set; }
    private bool _rightToLeftLayout = false;

    protected override async Task OnInitializedAsync()
    {
        var claimsPrincipal = (await _authenticationStateTask).User;
        if (claimsPrincipal.Identity?.IsAuthenticated ?? false) _navigationManager.NavigateTo("/");

        if (await _localStorage.ContainKeyAsync("ThemeManager"))
            _themeManagerModel = await _localStorage.GetItemAsync<ThemeManagerModel>("ThemeManager");
        else
            _themeManagerModel ??= new();

        _rightToLeftLayout = _themeManagerModel.CurrentCulture == "fa-IR";
        CultureInfo cultureInfo = new(_themeManagerModel.CurrentCulture);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }

    private readonly MudTheme _theme = new()
    {
        PaletteLight = new() { Primary = Colors.Green.Default },
        LayoutProperties = new() { AppbarHeight = "80px", DefaultBorderRadius = "12px" }
    };

    private async Task ChangeLanguage(ChangeEventArgs e)
    {
        if (e.Value.ToString() == "--" || e.Value.ToString() == _themeManagerModel.LanguageCode) return;
        _themeManagerModel.CurrentCulture = e.Value.ToString();
        _themeManagerModel.LanguageCode = e.Value.ToString().Split('-')[0].ToUpper();
        await _localStorage.SetItemAsync("ThemeManager", _themeManagerModel);
        _rightToLeftLayout = _themeManagerModel.CurrentCulture == "fa-IR";
        await _localStorage.SetItemAsStringAsync("RTLLayout", _rightToLeftLayout ? "RTL" : "LTR");

        CultureInfo cultureInfo = new(_themeManagerModel.CurrentCulture);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        StateHasChanged();
    }
}
