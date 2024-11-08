using Dotnetable.Admin.Components.PageComponents.Member.Profile;
using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.Pages.Member;

public partial class MemberProfile
{
    [Inject] private IDialogService _dialogService { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private MemberService _member { get; set; }
    [Inject] private Tools _tools { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private Dotnetable.Shared.DTO.Member.MemberDetailResponse _memberDetail { get; set; } = new();
    private Dotnetable.Shared.DTO.Member.MemberEditRequest _memberEdit { get; set; } = new();


    protected async override Task OnInitializedAsync()
    {
        int memberID = await _tools.GetRequesterMemberID();
        var fetchMemberDetail = await _member.MemberDetail(new() { CurrentMemberID = memberID });
        if (fetchMemberDetail.ErrorException is null)
        {
            _memberEdit = fetchMemberDetail.CastModel<Dotnetable.Shared.DTO.Member.MemberEditRequest>();
        }
    }

    private async Task InsertNewContact()
    {
        var checkInsert = await _dialogService.Show<ContactMemberDialog>(_loc["_Member_Insert_New_Address"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }, parameters: new() { { "ContactModel", null }, { "FunctionName", "ContactInsert" } }).Result;
        if (!checkInsert.Canceled)
            _memberDetail.Addresses.Add(checkInsert.Data.CastModel<Dotnetable.Shared.DTO.Member.MemberContactRequest>());
    }

}
