using Dotnetable.Admin.Components.Shared.Dialogs;
using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.Pages.Member.Policy;

public partial class PolicyManage
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IDialogService _dialogService { get; set; }
    [Inject] private MemberService _member { get; set; }
    [Inject] private Tools _tools { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private async Task InsertPolicy()
    {
        var promptResponse = await _dialogService.Show<PromptDialog>(_loc["_InsertPolicy"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }, parameters: new() { { "ColumnTitle", (_loc["_Title"]).ToString() } }).Result;
        if (promptResponse.Canceled) return;
        int memberID = await _tools.GetRequesterMemberID();

        var fetchResponse = await _member.PolicyInsert(new() { Title = promptResponse.Data.ToString(), CurrentMemberID = memberID });
        if (fetchResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Insert"]}", Severity.Success);
        }
        else
        {
            _snackbar.Add($"{((fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"])} {_loc["_Insert"]}", Severity.Error);
        }
    }


}
