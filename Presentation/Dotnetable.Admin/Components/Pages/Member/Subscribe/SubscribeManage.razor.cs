using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.Pages.Member.Subscribe;

public partial class SubscribeManage
{
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private MemberService _member { get; set; }
    [Inject] private Tools _tools { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }


    private GridViewHeaderParameters _gridHeaderParams { get; set; }
    private SubscribeListRequest _requestModel { get; set; }
    private SubscribeListResponse _responseModel { get; set; }
    int memberID = -1;

    protected async override Task OnInitializedAsync()
    {
        memberID = await _tools.GetRequesterMemberID();
        _requestModel = new() { CurrentMemberID = memberID };
        _gridHeaderParams = new()
        {
            HeaderItems = new()
                {
                    new() { ColumnLocalizeCode = "_EmailSubscribeID", ColumnName = nameof(SubscribeListResponse.SubscribeDetail.EmailSubscribeID), HasSearch = false, HasSort = true },
                    new() { ColumnLocalizeCode = "_Email", ColumnName = nameof(SubscribeListResponse.SubscribeDetail.Email), HasSearch = true, SearchType = SearchColumnType.Text, HasSort = true },
                    new() { ColumnLocalizeCode = "_MemberID", ColumnName = nameof(SubscribeListResponse.SubscribeDetail.MemberID), HasSearch = true, SearchType = SearchColumnType.Text, HasSort = false },
                    new() { ColumnLocalizeCode = "_RegisterDate", ColumnName = nameof(SubscribeListResponse.SubscribeDetail.LogTime), HasSearch = false, HasSort = true },
                    new() { ColumnLocalizeCode = "_Active_DeActive", ColumnName = nameof(SubscribeListResponse.SubscribeDetail.Active), HasSearch = true, SearchType = SearchColumnType.DropDown, OtherDropDownValues = new() { { "Active", "1" }, { "DeActive", "0" } }, HasSort = false },
                    new() { ColumnLocalizeCode = "_Management" }
                },
            Pagination = new() { MaxLength = _responseModel?.DatabaseRecords ?? 1, ShowFirstLast = true }
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
        string searchMemberID = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(SubscribeListResponse.SubscribeDetail.MemberID)).SearchText ?? "";
        _requestModel = new()
        {
            SkipCount = ((_gridHeaderParams.Pagination.PageIndex - 1) * _gridHeaderParams.Pagination.PageSize),
            TakeCount = _gridHeaderParams.Pagination.PageSize,
            OrderbyParams = _gridHeaderParams.OrderbyParams,
            Email = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(SubscribeListResponse.SubscribeDetail.Email)).SearchText,
            Active = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(SubscribeListResponse.SubscribeDetail.Active)).SearchText switch { "1" => true, "0" => false, _ => null },
            MemberID = string.IsNullOrEmpty(searchMemberID) || searchMemberID == "" ? null : Convert.ToInt32(searchMemberID),
            CurrentMemberID = memberID
        };
    }

    private async Task FetchGrid()
    {
        var fetchResponse = await _member.MemberSubscribedList(_requestModel);
        if (fetchResponse.ErrorException is null)
            _responseModel = fetchResponse;
        
        _gridHeaderParams.Pagination.MaxLength = _responseModel?.DatabaseRecords ?? 1;
        StateHasChanged();
    }

    private async Task ChangeActiveStatus(SubscribeListResponse.SubscribeDetail requestModel)
    {
        var changeResponse = await _member.MemberSubscribedChangeStatus(new(){ EmailSubscribeID = requestModel.EmailSubscribeID, CurrentMemberID = memberID });
        if (!changeResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Active_DeActive"]}", Severity.Error);
            return;
        }

        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Active_DeActive"]}", Severity.Success);
        requestModel.Active = !requestModel.Active;
        return;
    }




}

