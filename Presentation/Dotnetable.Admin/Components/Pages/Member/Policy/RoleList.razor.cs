using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.DTO.Public;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Dotnetable.Admin.Components.Pages.Member.Policy;

public partial class RoleList
{

    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private MemberService _member { get; set; }
    [Inject] private Tools _tools { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private List<RoleListResponse.RoleDetail> _cachedRoles { get; set; }
    private GridViewHeaderParameters _gridHeaderParams { get; set; }

    protected async override Task OnInitializedAsync()
    {
        int memberID = await _tools.GetRequesterMemberID();
        var fetchServiceData = await _member.RoleList(new() { CurrentMemberID = memberID});
        if (fetchServiceData.ErrorException is null)
        {
            _cachedRoles = fetchServiceData.Roles;
        }

        _gridHeaderParams = new()
        {
            HeaderItems = new()
            {
                new() { ColumnLocalizeCode = "_RoleID" },
                new() { ColumnLocalizeCode = "_RoleKey" },
                new() { ColumnLocalizeCode = "_Description" }
            },
            Pagination = new() { MaxLength = 1, ShowFirstLast = true }
        };
    }

    private void OnSearchChanged(GridViewHeaderParameters changedColumns)
    {
    }





}
