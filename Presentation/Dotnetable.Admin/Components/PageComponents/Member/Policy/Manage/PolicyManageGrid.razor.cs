using Dotnetable.Admin.Components.Shared.Dialogs;
using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.DTO.Public;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Member.Policy.Manage;

public partial class PolicyManageGrid
{
    [Inject] private MemberService _member { get; set; }
    [Inject] private Tools _tools { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IDialogService _dialogService { get; set; }

    private PolicyListRequest _policyListRequest { get; set; } = new();
    private PolicyListResponse _policyListResponse { get; set; }
    private GridViewHeaderParameters _gridHeaderParams { get; set; }

    protected async override Task OnInitializedAsync()
    {
        _policyListRequest = new();
        _gridHeaderParams = new()
        {
            HeaderItems = new()
            {
                new() { ColumnTitle = _loc["_PolicyID"], ColumnName = nameof(PolicyListResponse.PolicyDetail.PolicyID), HasSort = true },
                new() { ColumnTitle = _loc["_Title"], ColumnName = nameof(PolicyListResponse.PolicyDetail.Title), HasSearch = true, SearchType = SearchColumnType.Text, HasSort = true },
                new() { ColumnTitle = _loc["_Active_DeActive"] },
                new() { ColumnTitle = _loc["_Management"] }
            },
            Pagination = new() { MaxLength = _policyListResponse?.DatabaseRecords ?? 1, ShowFirstLast = true }
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

    private async void RefreshRequestInput()
    {
        _policyListRequest = new PolicyListRequest()
        {
            SkipCount = (_gridHeaderParams.Pagination.PageIndex - 1) * _gridHeaderParams.Pagination.PageSize,
            TakeCount = _gridHeaderParams.Pagination.PageSize,
            OrderbyParams = _gridHeaderParams.OrderbyParams,
            Title = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(PolicyListResponse.PolicyDetail.Title)).SearchText, 
            CurrentMemberID = await _tools.GetRequesterMemberID()
        };
    }

    private async Task FetchGrid()
    {
        var fetchPolicies = await _member.PolicyList(_policyListRequest);
        if (fetchPolicies.ErrorException is null)
            _policyListResponse = fetchPolicies;

        _gridHeaderParams.Pagination.MaxLength = _policyListResponse?.DatabaseRecords ?? 1;
        StateHasChanged();
    }
    #endregion


    #region CRUD
    private async Task ChangeActiveStatus(PolicyListResponse.PolicyDetail requestModel)
    {
        var fetchResponse = await _member.PolicyChangeStatus(new() { PolicyID = requestModel.PolicyID, CurrentMemberID = await _tools.GetRequesterMemberID() });
        if (fetchResponse.SuccessAction)
        {
            requestModel.Active = !requestModel.Active;
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Active_DeActive"]}", Severity.Success);
        }
        else
        {
            _snackbar.Add($"{((fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"])} {_loc["_Active_DeActive"]}", Severity.Error);
        }
    }

    private async Task UpdatePolicy(PolicyListResponse.PolicyDetail requestModel)
    {
        var promptResponse = await _dialogService.Show<PromptDialog>(_loc["_UpdatePolicy"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }, parameters: new() { { "ColumnTitle", _loc["_Title"].ToString() }, { "DefaultValue", requestModel.Title } }).Result;
        if (promptResponse.Canceled) return;

        requestModel.Title = promptResponse.Data.ToString();

        //var FetchPolicyItem = (from i in PolicyListResponse.Policies where i.PolicyID == SelectedPolicyID select i).FirstOrDefault();
        //if (FetchPolicyItem is null) return;

        var fetchResponse = await _member.PolicyUpdate(new() { PolicyID = requestModel.PolicyID, Title = requestModel.Title });
        if (fetchResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Update"]}", Severity.Success);
        }
        else
        {
            _snackbar.Add($"{((fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"])} {_loc["_Update"]}", Severity.Error);
        }
    }


    #endregion
}
