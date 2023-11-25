using Dotnetable.Admin.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

namespace Dotnetable.Admin.Components.PageComponents.Shared;

public partial class SideMenu
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private AuthenticationStateProvider _authenticationStateProvider { get; set; }
    [Inject] private NavigationManager _navigationManager { get; set; }

    [EditorRequired][Parameter] public bool SideMenuDrawerOpen { get; set; }
    [EditorRequired][Parameter] public EventCallback<bool> SideMenuDrawerOpenChanged { get; set; }
    [EditorRequired][Parameter] public bool CanMiniSideMenuDrawer { get; set; }
    [EditorRequired][Parameter] public EventCallback<bool> CanMiniSideMenuDrawerChanged { get; set; }
    [EditorRequired][Parameter] public UserModel User { get; set; }

    private async Task Logout()
    {
        await ((SharedServices.CustomAuthentication)_authenticationStateProvider).MarkUserAsLoggedOut();
        _navigationManager.NavigateTo("/Auth/Login");
    }

}