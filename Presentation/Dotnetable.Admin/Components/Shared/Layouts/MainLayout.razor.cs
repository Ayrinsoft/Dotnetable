using Blazored.LocalStorage;
using Dotnetable.Admin.Components.PageComponents.Shared;
using Dotnetable.Admin.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Globalization;

namespace Dotnetable.Admin.Components.Shared.Layouts;

public partial class MainLayout
{
    [Inject] private IDialogService _dialogService { get; set; }
    [Inject] private ILocalStorageService _localStorage { get; set; }
    [Inject] private IHttpContextAccessor _httpContextAccessor { get; set; }
    [Inject] private IJSRuntime _jsRuntime { get; set; }

    private bool _canMiniSideMenuDrawer = true;
    private bool _commandPaletteOpen;
    private bool _sideMenuDrawerOpen;
    private bool _themingDrawerOpen;
    private bool _rightToLeftLayout = false;
    private HttpContext _context = null;
    private UserModel _user = new();
    private readonly PaletteLight _lightPalette = new()
    {
        Background = "#f5f5f5",
        BackgroundGrey = "#eeeeee",
        DrawerBackground = "#eeeeee",
        AppbarBackground = "#eeeeee",
        ActionDisabledBackground = "#a8a8a8",
    };

    private readonly PaletteDark _darkPalette = new()
    {
        //Black = "#27272f",
        //Background = "rgb(21,27,34)",
        //BackgroundGrey = "#27272f",
        //Surface = "#212B36",
        //DrawerBackground = "rgb(21,27,34)",
        //DrawerText = "rgba(255,255,255, 0.50)",
        //DrawerIcon = "rgba(255,255,255, 0.50)",
        //AppbarBackground = "#27272f",
        //AppbarText = "rgba(255,255,255, 0.70)",
        TextPrimary = "rgba(255,255,255, 0.70)",
        TextSecondary = "rgba(255,255,255, 0.50)",
        ActionDefault = "#adadb1",
        ActionDisabled = "rgba(255,255,255, 0.26)",
        //ActionDisabledBackground = "rgba(255,255,255, 0.12)",
        Divider = "rgba(255,255,255, 0.12)",
        DividerLight = "rgba(255,255,255, 0.06)",
        TableLines = "rgba(255,255,255, 0.12)",
        LinesDefault = "rgba(255,255,255, 0.12)",
        LinesInputs = "rgba(255,255,255, 0.3)",
        TextDisabled = "rgba(255,255,255, 0.2)"
    };


    private readonly MudTheme _theme = new()
    {
        Palette = new() { Primary = Colors.Green.Default },
        LayoutProperties = new() { AppbarHeight = "80px", DefaultBorderRadius = "12px" },
        Typography = new() { Default = new() { FontSize = "0.9rem", } }
    };
    private ThemeManagerModel _themeManager = new()
    {
        IsDarkMode = false,
        PrimaryColor = Colors.Green.Default,
        MaxWidthPage = MaxWidth.ExtraExtraLarge
    };

    protected override async Task OnInitializedAsync()
    {
        _context = _httpContextAccessor.HttpContext;
        if (await _localStorage.ContainKeyAsync("MemberAuthorized"))
        {
            var fetchAuthorize = await _localStorage.GetItemAsync<Dotnetable.Shared.DTO.Authentication.UserLoginResponse>("MemberAuthorized");
            _user = new UserModel()
            {
                Avatar = !fetchAuthorize.AvatarID.HasValue ? "/images/avatar-m.jpg" : $"{_context.Request.Scheme}://{_context.Request.Host}/api/Files/Receive/120X120/{fetchAuthorize.AvatarID}/Avatar.png",
                DisplayName = $"{fetchAuthorize.Givenname} {fetchAuthorize.Surname}",
                Gender = fetchAuthorize.Gender ?? true,
                Email = fetchAuthorize.Email,
                Role = fetchAuthorize.PolicyName,
                LanguageCode = fetchAuthorize.LanguageCode
            };
        }

        if (await _localStorage.ContainKeyAsync("ThemeManager"))
            _themeManager = await _localStorage.GetItemAsync<ThemeManagerModel>("ThemeManager");
        else
            _themeManager ??= new();

        _themeManager.LanguageCode = _user.LanguageCode;

        await ThemeManagerChanged(_themeManager);

        if (await _localStorage.ContainKeyAsync("RTLLayout"))
            _rightToLeftLayout = (await _localStorage.GetItemAsStringAsync("RTLLayout")) == "RTL";

        await RightToLeftLayoutChanged();
    }

    private void ToggleSideMenuDrawer() => _sideMenuDrawerOpen = !_sideMenuDrawerOpen;

    private async Task RightToLeftLayoutChanged()
    {
        _rightToLeftLayout = _themeManager.CurrentCulture == "fa-IR";
        await _localStorage.SetItemAsStringAsync("RTLLayout", (_rightToLeftLayout ? "RTL" : "LTR"));

        CultureInfo cultureInfo = new(_themeManager.CurrentCulture);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        //await InvokeAsync(StateHasChanged);
    }


    private async Task ThemeManagerChanged(ThemeManagerModel themeManager)
    {
        _themeManager = themeManager;
        _theme.Palette = _themeManager.IsDarkMode ? _darkPalette : _lightPalette;
        _theme.Palette.Primary = _themeManager.PrimaryColor;
        await UpdateThemeManagerLocalStorage();
    }

    private async Task OpenCommandPalette()
    {
        if (!_commandPaletteOpen)
        {
            DialogOptions options = new() { NoHeader = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            var commandPalette = _dialogService.Show<CommandPalette>("", options);
            _commandPaletteOpen = true;
            await commandPalette.Result;
            _commandPaletteOpen = false;
        }
    }

    private async Task UpdateThemeManagerLocalStorage()
    {
        await _localStorage.SetItemAsync("ThemeManager", _themeManager);
    }
}
