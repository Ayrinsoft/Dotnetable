using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Dotnetable.Admin.Components.PageComponents.Member.Policy;

public partial class PolicySelector
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }

    [Parameter] public EventCallback<PolicyListOnInsertMemberResponse> OnChangePolicyEvent { get; set; }
    [Parameter] public int? PassedPolicyID { get; set; } = 0;

    private PolicyListOnInsertMemberResponse _selectedPolicy { get; set; } = null;
    private List<PolicyListOnInsertMemberResponse> _policies { get; set; } = new();


    protected override async Task OnInitializedAsync()
    {
        var fetchPolicies = await _httpService.CallServiceObjAsync(HttpMethod.Get, true, "Member/PolicyListOnMemberManage");
        if (fetchPolicies.Success)
        {
            _policies = fetchPolicies.ResponseData.CastModel<List<PolicyListOnInsertMemberResponse>>();
        }
        if (PassedPolicyID.HasValue && _selectedPolicy is null) _selectedPolicy = _policies.Where(i => i.PolicyID == PassedPolicyID).FirstOrDefault();
    }

    private async Task<IEnumerable<PolicyListOnInsertMemberResponse>> SearchPolicy(string searchKey)
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
