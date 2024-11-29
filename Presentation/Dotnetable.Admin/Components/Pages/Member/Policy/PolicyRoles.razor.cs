using Dotnetable.Admin.Components.PageComponents.Member.Policy;
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

public partial class PolicyRoles
{
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private MemberService _member { get; set; }
    [Inject] private Tools _tools { get; set; }
    [Inject] private IDialogService _dialogService { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    [Parameter] public int PolicyID { get; set; }

    private PolicyRoleListRequest _policyRolesListRequest { get; set; }
    private PolicyRoleListResponse _policyRolesListResponse { get; set; }
    private GridViewHeaderParameters _gridHeaderParams { get; set; }
    int memberID = -1;
    protected async override Task OnInitializedAsync()
    {
        memberID = await _tools.GetRequesterMemberID();
        _policyRolesListRequest = new() { CurrentMemberID = memberID };
        _gridHeaderParams = new()
        {
            HeaderItems = new()
            {
                new(){ ColumnLocalizeCode = "_PolicyRoleID", ColumnName = nameof(PolicyRoleListResponse.RoleDetail.PolicyRoleID), HasSort = true },
                new(){ ColumnLocalizeCode = "_RoleID", ColumnName = nameof(PolicyRoleListResponse.RoleDetail.RoleID), HasSort = true},
                new(){ ColumnLocalizeCode = "_RoleKey", ColumnName = nameof(PolicyRoleListResponse.RoleDetail.RoleKey), HasSort = true },
                new(){ ColumnLocalizeCode = "_Management" }
            },
            Pagination = new() { MaxLength = _policyRolesListResponse?.DatabaseRecords ?? 1, ShowFirstLast = true }
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
        _policyRolesListRequest = new PolicyRoleListRequest()
        {
            SkipCount = ((_gridHeaderParams.Pagination.PageIndex - 1) * _gridHeaderParams.Pagination.PageSize),
            TakeCount = _gridHeaderParams.Pagination.PageSize,
            OrderbyParams = _gridHeaderParams.OrderbyParams,
            PolicyID = PolicyID,
            CurrentMemberID = memberID
        };
    }

    private async Task FetchGrid()
    {
        var fetchPolicies = await _member.PolicyRolesList(_policyRolesListRequest);
        if (fetchPolicies.ErrorException is null)
        {
            _policyRolesListResponse = fetchPolicies;
        }
        _gridHeaderParams.Pagination.MaxLength = _policyRolesListResponse?.DatabaseRecords ?? 1;
        StateHasChanged();
    }
    #endregion


    private async Task RemoveItem(PolicyRoleListResponse.RoleDetail requestModel)
    {
        var fetchResponse = await _member.PolicyRoleRemove(new() { CurrentMemberID = memberID, PolicyRoleID = requestModel.PolicyRoleID });
        if (fetchResponse.SuccessAction)
        {
            var fetchPolicyRole = (from i in _policyRolesListResponse.Roles where i.PolicyRoleID == requestModel.PolicyRoleID select i).FirstOrDefault();
            if (fetchPolicyRole is not null) _policyRolesListResponse.Roles.Remove(fetchPolicyRole);

            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Remove"]}", Severity.Success);
        }
        else
        {
            string errorCode = (fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"];
            _snackbar.Add($"{errorCode} {_loc["_Remove"]}", Severity.Error);
        }
    }

    private async Task AppendRoleToPolicy()
    {
        var promptResponse = await _dialogService.Show<RoleSelectorDialog>($"{_loc["_Append"]} {_loc["_RoleKey"]}", options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }).Result;
        if (promptResponse.Canceled) return;

        var dialogresponseData = promptResponse.Data.CastModel<RoleListOnPolicyManageResponse>();
        if (dialogresponseData is null) return;

        var fetchResponse = await _member.MemberRoleAppend(new() { CurrentMemberID = memberID, PolicyID = PolicyID, RoleKey = dialogresponseData.RoleKey });
        if (fetchResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Remove"]}", Severity.Success);
            await FetchGrid();
        }
        else
        {
            string errorCode = (fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"];
            _snackbar.Add($"{errorCode} {_loc["_Remove"]}", Severity.Error);
        }
    }

}
