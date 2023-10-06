using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dotnetable.Admin.Sahred.Auth;

public partial class LoginLayout
{
    [CascadingParameter] private Task<AuthenticationState> _authenticationStateTask { get; set; }
    [Inject] private NavigationManager _navigationManager { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var claimsPrincipal = (await _authenticationStateTask).User;
        if (claimsPrincipal.Identity?.IsAuthenticated ?? false) _navigationManager.NavigateTo("/");
    }

    private readonly MudTheme _theme = new()
    {
        Palette = new() { Primary = Colors.Green.Default },
        LayoutProperties = new() { AppbarHeight = "80px", DefaultBorderRadius = "12px" },
        Typography = new() { Default = new() { FontSize = "0.9rem", } }
    };
}
