using Dotnetable.Admin.Components.PageComponents.Messages.EmailSetting;
using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.DTO.Message;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.Pages.Messages;

public partial class EmailSettingManage
{
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [Inject] private IDialogService _dialogService { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private EmailPanelListRequest _listRequest { get; set; }
    private EmailPanelListResponse _listResponse { get; set; }
    private GridViewHeaderParameters _gridHeaderParams { get; set; }

    protected async override Task OnInitializedAsync()
    {
        _gridHeaderParams = new()
        {
            HeaderItems = new()
            {
                new() { ColumnLocalizeCode = "_EmailSettingID", ColumnName = nameof(EmailPanelListResponse.EmailSettingDetail.EmailSettingID), HasSort = true },
                new() { ColumnLocalizeCode = "_EmailAddress" },
                new() { ColumnLocalizeCode = "_SMTPPort" },
                new() { ColumnLocalizeCode = "_MailServer" },
                new() { ColumnLocalizeCode = "_EmailName", ColumnName = nameof(EmailPanelListResponse.EmailSettingDetail.EmailName), HasSearch = true, SearchType = SearchColumnType.Text, HasSort = true },
                new() { ColumnLocalizeCode = "_EMailType" },
                new() { ColumnLocalizeCode = "_Active_DeActive" },
                new() { ColumnLocalizeCode = "_Management" }
            },
            Pagination = new() { MaxLength = _listResponse?.DatabaseRecords ?? 1, ShowFirstLast = true }
        };

        RefreshRequestInput();
        await FetchGrid();
    }


    #region GRID

    private async Task OnSearchChanged(GridViewHeaderParameters changedColumns)
    {
        _gridHeaderParams = changedColumns;
        RefreshRequestInput();
        await FetchGrid();
    }

    private void RefreshRequestInput()
    {
        _listRequest = new()
        {
            SkipCount = ((_gridHeaderParams.Pagination.PageIndex - 1) * _gridHeaderParams.Pagination.PageSize),
            TakeCount = _gridHeaderParams.Pagination.PageSize,
            OrderbyParams = _gridHeaderParams.OrderbyParams,
            EmailName = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(EmailPanelListResponse.EmailSettingDetail.EmailName))?.SearchText ?? ""
        };
    }

    private async Task FetchGrid()
    {
        var fetchList = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Message/EmailSettingList", _listRequest.ToJsonString());
        if (fetchList.Success)
        {
            _listResponse = fetchList.ResponseData.CastModel<EmailPanelListResponse>();
        }
        _gridHeaderParams.Pagination.MaxLength = _listResponse?.DatabaseRecords ?? 1;
        StateHasChanged();
    }
    #endregion



    public async Task InsertNewSetting()
    {
        var checkDialogData = await _dialogService.Show<EmailSettingDialog>(_loc["_Email_Setting_Insert"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true, FullWidth = true, FullScreen = true }, parameters: new() { { "FormModel", new EmailPanelUpdateRequest()} }).Result;
        if (checkDialogData.Canceled) return;

        var dialogresponseData = checkDialogData.Data.CastModel<EmailPanelUpdateRequest>();
        if (dialogresponseData is null) return;

        var updateOnAPIResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Message/EmailSettingInsert", dialogresponseData.ToJsonString());
        if (!updateOnAPIResponse.Success)
        {
            _snackbar.Add($"{((updateOnAPIResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{updateOnAPIResponse.ErrorException?.ErrorCode}"])} {_loc["_Email_Setting_Insert"]}", Severity.Error);
            return;
        }

        var parsedResponse = updateOnAPIResponse.ResponseData.CastModel<PublicActionResponse>();
        if (!parsedResponse.SuccessAction)
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Email_Setting_Insert"]}", Severity.Error);

        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Email_Setting_Insert"]}", Severity.Success);
    }

    private async Task ChangeActiveStatus(EmailPanelListResponse.EmailSettingDetail requestModel)
    {
        var fetchResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Message/EmailSettingChangeStatus", new EmailPanelChangeStatusRequest { EmailSettingID = requestModel.EmailSettingID }.ToJsonString());
        if (!fetchResponse.Success)
        {
            _snackbar.Add($"{((fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"])} {_loc["_Active_DeActive"]}", Severity.Error);
            return;
        }

        var parsedResponse = fetchResponse.ResponseData.CastModel<PublicActionResponse>();
        if (!parsedResponse.SuccessAction) return;

        requestModel.Active = !requestModel.Active;
        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Active_DeActive"]}", Severity.Success);
    }

    private async Task EditCurrentSetting(EmailPanelListResponse.EmailSettingDetail requestModel)
    {
        var checkDialogData = await _dialogService.Show<EmailSettingDialog>(_loc["_Email_Setting_Update"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true, FullWidth = true, FullScreen = true }, parameters: new() { { "FormModel", requestModel.CastModel<EmailPanelUpdateRequest>() } }).Result;
        if (checkDialogData.Canceled) return;

        var dialogresponseData = checkDialogData.Data.CastModel<EmailPanelUpdateRequest>();
        if (dialogresponseData is null) return;

        dialogresponseData.EmailSettingID = requestModel.EmailSettingID;

        var updateOnAPIResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Message/EmailSettingUpdate", dialogresponseData.ToJsonString());
        if (!updateOnAPIResponse.Success)
        {
            _snackbar.Add($"{((updateOnAPIResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{updateOnAPIResponse.ErrorException?.ErrorCode}"])} {_loc["_Email_Setting_Update"]}", Severity.Error);
            return;
        }

        var parsedResponse = updateOnAPIResponse.ResponseData.CastModel<PublicActionResponse>();
        if (!parsedResponse.SuccessAction) return;

        try
        {
            requestModel.EnableSSL = dialogresponseData.EnableSSL ?? false;
            requestModel.SMTPPort = dialogresponseData.SMTPPort ?? 0;
            requestModel.EmailName = dialogresponseData.EmailName;
            requestModel.DefaultEmail = dialogresponseData.DefaultEmail ?? false;
            requestModel.EmailAddress = dialogresponseData.EmailAddress;
            requestModel.EmailPassword = dialogresponseData.EmailPassword;
            requestModel.EMailType = (byte)dialogresponseData.EMailType; 
            requestModel.MailServer = dialogresponseData.MailServer;
        }
        catch (Exception) { }

        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Email_Setting_Update"]}", Severity.Success);
    }


}
