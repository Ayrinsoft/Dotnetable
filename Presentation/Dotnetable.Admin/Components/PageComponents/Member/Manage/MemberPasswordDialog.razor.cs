using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Member.Manage;

public partial class MemberPasswordDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }

    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [Parameter] public int CurrentMemberID { get; set; }


    private string _newPassword = "";

    private async Task OnAcceptChangePassword()
    {
        var checkNewPass = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,25}$").Match(_newPassword).Success;
        if (!checkNewPass)
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Err_Member_Password_Policy"]}", Severity.Error);
            return;
        }

        MemberChangePasswordAdminRequest changeRequest = new()
        {
            MemberID = CurrentMemberID,
            NewPassword = _newPassword,
            SendMailForUser = true
        };
        var changeResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Member/ChangeUserPassword", changeRequest.ToJsonString());
        if (changeResponse.Success)
        {
            var parsedChangePassword = changeResponse.ResponseData.CastModel<PublicActionResponse>();
            if (parsedChangePassword.SuccessAction)
            {
                _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_ChangePassword"]}", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                return;
            }
            else if (parsedChangePassword.ErrorException != null && !string.IsNullOrEmpty(parsedChangePassword.ErrorException.ErrorCode))
            {
                _snackbar.Add($"{_loc[$"_ERROR_{parsedChangePassword.ErrorException.ErrorCode}"]} {_loc["_ChangePassword"]}", Severity.Error);
                return;
            }
        }
        _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_ChangePassword"]}", Severity.Error);
    }

    private void GenerateNewPassword()
    {
        _newPassword = GeneralEvents.GenerateRandomPassword(10);
    }

    private async Task SubmitForm(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            await OnAcceptChangePassword();
    }

}
