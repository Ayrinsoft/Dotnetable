using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.Tools;
using Dotnetable.Shared.DTO.Public;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Dotnetable.Admin.Models.DTO.Member;

namespace Dotnetable.Admin.Components.Pages.Member;

public partial class ChangePassword
{
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IStringLocalizer<Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private MemberChangePasswordRequest _changeRequest = new();
    private string _confirmPassword = "";

    private async Task DoChangePassword()
    {
        if (_changeRequest.NewPassword != _confirmPassword) return;
        var changePasswordResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Member/ChangeSelfPassword", _changeRequest.ToJsonString());
        if (!changePasswordResponse.Success)
        {
            string errorCode = (changePasswordResponse.ErrorException is null ? _loc["_FailedAction"] : _loc[$"_ERROR_{changePasswordResponse.ErrorException.ErrorCode}"]);
            _snackbar.Add($"{errorCode} {_loc["_ChangePassword"]}", Severity.Error);
            return;
        }

        var changeObject = changePasswordResponse.ResponseData.CastModel<PublicActionResponse>();
        if (!changeObject.SuccessAction)
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_ChangePassword"]}", Severity.Error);
        }
        else
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_ChangePassword"]}", Severity.Success);
        }
    }

}
