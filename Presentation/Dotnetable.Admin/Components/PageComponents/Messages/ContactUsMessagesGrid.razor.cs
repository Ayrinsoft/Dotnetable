using Dotnetable.Admin.Components.Shared.Dialogs;
using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Message;
using Dotnetable.Shared.DTO.Public;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Messages;

public partial class ContactUsMessagesGrid
{

    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private MessageService _message { get; set; }
    [Inject] private Tools _tools { get; set; }
    [Inject] private IDialogService _dialogService { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }


    private GridViewHeaderParameters _gridHeaderParams { get; set; }
    private MessageContactUsListResponse _gridData { get; set; }
    private MessageContactUsListRequest _gridRequest { get; set; }
    int memberID = -1;

    protected async override Task OnInitializedAsync()
    {
        memberID = await _tools.GetRequesterMemberID();

        _gridRequest = new() { CurrentMemberID = memberID };
        _gridHeaderParams = new()
        {
            HeaderItems = new()
            {
                new() { ColumnLocalizeCode = "_ContactUsMessagesID", ColumnName = nameof(MessageContactUsListResponse.ContactMessage.ContactUsMessagesID), HasSort = true },
                new() { ColumnLocalizeCode = "_Archive", ColumnName = nameof(MessageContactUsListResponse.ContactMessage.Archive), HasSearch = true, SearchType = SearchColumnType.DropDown, OtherDropDownValues = new() { { "Not Archive", "0" }, { "Archive", "1" } }, HasSort = true },
                new() { ColumnLocalizeCode = "_SenderName", ColumnName = nameof(MessageContactUsListResponse.ContactMessage.SenderName), HasSort = true },
                new() { ColumnLocalizeCode = "_EmailAddress", ColumnName = nameof(MessageContactUsListResponse.ContactMessage.EmailAddress), HasSort = true },
                new() { ColumnLocalizeCode = "_CellphoneNumber", ColumnName = nameof(MessageContactUsListResponse.ContactMessage.CellphoneNumber), HasSort = true },
                new() { ColumnLocalizeCode = "_MessageSubject" },
                new() { ColumnLocalizeCode = "_MessageBody" },
                new() { ColumnLocalizeCode = "_LogTime" },
                new() { ColumnLocalizeCode = "_SenderIPAddress" },
                new() { ColumnLocalizeCode = "_Management" }
            },
            Pagination = new() { MaxLength = _gridData?.DatabaseRecords ?? 1, ShowFirstLast = true }
        };

        RefreshRequestInput();
        await FetchGrid();
    }


    private async Task OnSearchChanged(GridViewHeaderParameters changedColumns)
    {
        _gridHeaderParams = changedColumns;
        RefreshRequestInput();
        await FetchGrid();
    }

    private void RefreshRequestInput()
    {
        _gridRequest = new()
        {
            SkipCount = (_gridHeaderParams.Pagination.PageIndex - 1) * _gridHeaderParams.Pagination.PageSize,
            TakeCount = _gridHeaderParams.Pagination.PageSize,
            OrderbyParams = _gridHeaderParams.OrderbyParams,
            Archive = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(MessageContactUsListResponse.ContactMessage.Archive)).SearchText switch { "1" => true, _ => null },
                CurrentMemberID = memberID
        };
    }

    private async Task FetchGrid()
    {
        var fetchList = await _message.ContactUsMessageList(_gridRequest);
        if (fetchList.ErrorException is null)
        {
            _gridData = fetchList;
        }
        _gridHeaderParams.Pagination.MaxLength = _gridData?.DatabaseRecords ?? 1;
        StateHasChanged();
    }

    private async Task ChangeArchiveStatus(int contactUsMessageID)
    {
        if ((await _dialogService.Show<ConfirmDialog>(_loc["_AreYouSure"], new DialogOptions { CloseOnEscapeKey = true, CloseButton = true, MaxWidth = MaxWidth.Small, Position = DialogPosition.Center }).Result).Canceled)
            return;

        var changeResponse = await _message.ContactUsMessageArchive(new() { DeleteItem = false, ContactUsMessageID = contactUsMessageID, CurrentMemberID = memberID });
        if (changeResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Archive"]}", Severity.Success);
            var FetchItem = (from i in _gridData.ContactUsMessages where i.ContactUsMessagesID == contactUsMessageID select i).FirstOrDefault();
            if (FetchItem is not null) FetchItem.Archive = !FetchItem.Archive;
            return;
        }
        _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Archive"]}", Severity.Error);

    }

    private async Task DeleteItem(int contactUsMessageID)
    {
        if ((await _dialogService.Show<ConfirmDialog>(_loc["_AreYouSure"], new DialogOptions { CloseOnEscapeKey = true, CloseButton = true, MaxWidth = MaxWidth.Small, Position = DialogPosition.Center }).Result).Canceled)
            return;

        var deleteResponse = await _message.ContactUsMessageDelete(new() { DeleteItem = true, ContactUsMessageID = contactUsMessageID, CurrentMemberID = memberID });
        if (deleteResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Delete"]}", Severity.Success);
            var FetchItem = (from i in _gridData.ContactUsMessages where i.ContactUsMessagesID == contactUsMessageID select i).FirstOrDefault();
            if (FetchItem is not null) _gridData.ContactUsMessages.Remove(FetchItem);
            return;
        }
        _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Delete"]}", Severity.Error);

    }

}
