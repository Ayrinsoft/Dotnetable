using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Member.Policy;

public partial class RoleSelectorDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }

    private RoleListOnPolicyManageResponse _selectedRole { get; set; }
    private List<RoleListOnPolicyManageResponse> _currentMemberRoles { get; set; } = new();

    private void OnSubmitObject()
    {
        if (_selectedRole is not null)
            MudDialog.Close(_selectedRole);
    }

    protected override async Task OnInitializedAsync()
    {
        var fetchRoles = await _httpService.CallServiceObjAsync(HttpMethod.Get, true, "Member/RoleListOnPolicyManage");
        if (fetchRoles.Success)
            _currentMemberRoles = fetchRoles.ResponseData.CastModel<List<RoleListOnPolicyManageResponse>>();
    }

    private async Task<IEnumerable<RoleListOnPolicyManageResponse>> SearchRole(string searchKey, CancellationToken token)
    {
        if (string.IsNullOrEmpty(searchKey)) return await Task.FromResult(_currentMemberRoles);
        return await Task.FromResult(_currentMemberRoles.Where(i => i.RoleKey.Contains(searchKey, StringComparison.InvariantCultureIgnoreCase)));
    }

    private void RoleSelectedChange(RoleListOnPolicyManageResponse selectedItem)
    {
        _selectedRole = selectedItem;
    }

}
