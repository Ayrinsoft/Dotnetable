using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Member;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Member.Policy;

public partial class RoleSelectorDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private MemberService _member { get; set; }
    [Inject] private Tools _tools { get; set; }

    private RoleListOnPolicyManageResponse _selectedRole { get; set; }
    private List<RoleListOnPolicyManageResponse> _currentMemberRoles { get; set; } = new();

    private void OnSubmitObject()
    {
        if (_selectedRole is not null)
            MudDialog.Close(_selectedRole);
    }

    protected override async Task OnInitializedAsync()
    {
        int memberID = await _tools.GetRequesterMemberID();
        _currentMemberRoles = await _member.RoleListOnPolicyManage(memberID);
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
