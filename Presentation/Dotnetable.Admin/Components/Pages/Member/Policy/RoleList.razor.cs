using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Dotnetable.Admin.Components.Pages.Member.Policy;

public partial class RoleList
{

    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private List<RoleListResponse.RoleDetail> _cachedRoles { get; set; }
    private GridViewHeaderParameters _gridHeaderParams { get; set; }

    protected async override Task OnInitializedAsync()
    {
        var fetchServiceData = await _httpService.CallServiceObjAsync(HttpMethod.Get, true, "Member/RoleList");
        if (fetchServiceData.Success)
        {
            _cachedRoles = fetchServiceData.ResponseData.CastModel<RoleListResponse>().Roles;
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
