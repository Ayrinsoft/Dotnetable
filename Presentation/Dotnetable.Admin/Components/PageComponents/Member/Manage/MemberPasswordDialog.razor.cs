using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Member.Manage;

public partial class MemberPasswordDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }

    [Inject] private MemberService _member { get; set; }
    [Inject] private Tools _tools { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Parameter] public int SelectedMemberID { get; set; }
    int currentMemberID = -1;

    protected override async Task OnInitializedAsync()
    {
        currentMemberID = await _tools.GetRequesterMemberID();
    }

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
            MemberID = SelectedMemberID,
            NewPassword = _newPassword,
            SendMailForUser = true,
            CurrentMemberID = currentMemberID
        };
        var changeResponse = await _member.ChangeUserPassword(changeRequest);
        if (changeResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_ChangePassword"]}", Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
            return;
        }
        else if (changeResponse.ErrorException != null && !string.IsNullOrEmpty(changeResponse.ErrorException.ErrorCode))
        {
            _snackbar.Add($"{_loc[$"_ERROR_{changeResponse.ErrorException.ErrorCode}"]} {_loc["_ChangePassword"]}", Severity.Error);
            return;
        }
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
