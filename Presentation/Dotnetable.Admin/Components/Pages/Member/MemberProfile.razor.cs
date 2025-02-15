using Dotnetable.Admin.Components.PageComponents.Member.Profile;
using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.Pages.Member;

public partial class MemberProfile
{
    [Inject] private IDialogService _dialogService { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private Dotnetable.Shared.DTO.Member.MemberDetailResponse _memberDetail { get; set; } = new();
    private Dotnetable.Shared.DTO.Member.MemberEditRequest _memberEdit { get; set; } = new();


    protected async override Task OnInitializedAsync()
    {
        var fetchMemberDetail = await _httpService.CallServiceObjAsync(HttpMethod.Get, true, "Member/MemberDetail");
        if (fetchMemberDetail.Success)
        {
            _memberDetail = fetchMemberDetail.ResponseData.CastModel<Dotnetable.Shared.DTO.Member.MemberDetailResponse>();
            _memberEdit = _memberDetail.CastModel<Dotnetable.Shared.DTO.Member.MemberEditRequest>();
        }
    }

    private async Task InsertNewContact()
    {
        var checkInsert = await _dialogService.Show<ContactMemberDialog>(_loc["_Member_Insert_New_Address"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }, parameters: new() { { "ContactModel", null }, { "FunctionName", "ContactInsert" } }).Result;
        if (!checkInsert.Canceled)
            _memberDetail.Addresses.Add(checkInsert.Data.CastModel<Dotnetable.Shared.DTO.Member.MemberContactRequest>());
    }

}
