using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Pages.Member;

public partial class MemberProfile
{
    [Inject] private IDialogService _dialogService { get; set; }
    [Inject] private IStringLocalizer<Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private Shared.DTO.Member.MemberDetailResponse _memberDetail { get; set; } = new();
    private Shared.DTO.Member.MemberEditRequest _memberEdit { get; set; } = new();


    protected async override Task OnInitializedAsync()
    {
        var fetchMemberDetail = await _httpService.CallServiceObjAsync(HttpMethod.Get, true, "Member/MemberDetail");
        if (fetchMemberDetail.Success)
        {
            _memberDetail = fetchMemberDetail.ResponseData.CastModel<Shared.DTO.Member.MemberDetailResponse>();
            _memberEdit = _memberDetail.CastModel<Shared.DTO.Member.MemberEditRequest>();
        }
    }

    private async Task InsertNewContact()
    {
        var checkInsert = await _dialogService.Show<Components.Member.Profile.ContactMemberDialog>(_loc["_Member_Insert_New_Address"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }, parameters: new() { { "ContactModel", null }, { "FunctionName", "ContactInsert" } }).Result;
        if (!checkInsert.Canceled)
            _memberDetail.Addresses.Add(checkInsert.Data.CastModel<Shared.DTO.Member.MemberContactRequest>());
    }

}
