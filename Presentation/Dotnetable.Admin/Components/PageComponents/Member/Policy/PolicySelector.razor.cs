using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Dotnetable.Admin.Components.PageComponents.Member.Policy;

public partial class PolicySelector
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private MemberService _member { get; set; }
    [Inject] private Tools _tools { get; set; }

    [Parameter] public EventCallback<PolicyListOnInsertMemberResponse> OnChangePolicyEvent { get; set; }
    [Parameter] public int? PassedPolicyID { get; set; } = 0;

    private PolicyListOnInsertMemberResponse _selectedPolicy { get; set; } = null;
    private List<PolicyListOnInsertMemberResponse> _policies { get; set; } = new();


    protected override async Task OnInitializedAsync()
    {
        int memberID = await _tools.GetRequesterMemberID();
        _policies = await _member.PolicyListOnMemberManage(memberID);
        if (PassedPolicyID.HasValue && _selectedPolicy is null) _selectedPolicy = _policies.Where(i => i.PolicyID == PassedPolicyID).FirstOrDefault();
    }

    private async Task<IEnumerable<PolicyListOnInsertMemberResponse>> SearchPolicy(string searchKey, CancellationToken token)
    {
        if (string.IsNullOrEmpty(searchKey)) return await Task.FromResult(_policies);
        return await Task.FromResult(_policies.Where(i => i.Title.Contains(searchKey, StringComparison.InvariantCultureIgnoreCase)));
    }

    private async Task PolicySelectedChange(PolicyListOnInsertMemberResponse selectedItem)
    {
        _selectedPolicy = selectedItem;
        await OnChangePolicyEvent.InvokeAsync(selectedItem);
    }
}
