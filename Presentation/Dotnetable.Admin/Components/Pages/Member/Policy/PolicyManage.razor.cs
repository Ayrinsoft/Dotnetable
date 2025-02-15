using Dotnetable.Admin.Components.Shared.Dialogs;
using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices.Data;
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
    [Inject] private IHttpServices _httpService { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private async Task InsertPolicy()
    {
        var promptResponse = await _dialogService.Show<PromptDialog>(_loc["_InsertPolicy"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }, parameters: new() { { "ColumnTitle", (_loc["_Title"]).ToString() } }).Result;
        if (promptResponse.Canceled) return;
        
        var fetchResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Member/PolicyInsert", new PolicyInsertRequest { Title = promptResponse.Data.ToString() }.ToJsonString());
        if (fetchResponse.Success)
        {
            var parsedResponse = fetchResponse.ResponseData.CastModel<PublicActionResponse>();
            if (parsedResponse.SuccessAction)
            {
                _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Insert"]}", Severity.Success);
            }
        }
        else
        {
            _snackbar.Add($"{((fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"])} {_loc["_Insert"]}", Severity.Error);
        }
    }


}
