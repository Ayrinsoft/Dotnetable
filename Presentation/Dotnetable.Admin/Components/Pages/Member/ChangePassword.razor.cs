using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.Pages.Member;

public partial class ChangePassword
{
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private MemberService _member { get; set; }
    [Inject] private Tools _tools { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private Dotnetable.Shared.DTO.Member.MemberChangePasswordRequest _changeRequest = new();
    private string _confirmPassword = "";

    private async Task DoChangePassword()
    {
        if (_changeRequest.NewPassword != _confirmPassword) return;

        int memberID = await _tools.GetRequesterMemberID();
        _changeRequest.CurrentMemberID = memberID;

        var changePasswordResponse = await _member.ChangeSelfPassword(_changeRequest);
        if (!changePasswordResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_ChangePassword"]}", Severity.Error);
        }
        else
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_ChangePassword"]}", Severity.Success);
        }
    }

}
