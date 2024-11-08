using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Member.Profile.Contact;

public partial class MemberProfileContactGrid
{
    [Inject] private MemberService _member { get; set; }
    [Inject] private Tools _tools { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IDialogService _dialogService { get; set; }

    [Parameter] public List<MemberContactRequest> AddressList { get; set; }

    private GridViewHeaderParameters _gridHeaderParams { get; set; }

    protected override void OnInitialized()
    {
        _gridHeaderParams = new()
        {
            HeaderItems = new()
            {
                new() { ColumnTitle = _loc["_CityName"] },
                new() { ColumnTitle = _loc["_Address"] },
                new() { ColumnTitle = _loc["_CellphoneNumber"] },
                new() { ColumnTitle = _loc["_Management"] }
            },
            Pagination = new() { MaxLength = AddressList.Count, ShowFirstLast = true }
        };
    }

    private async Task UpdateAddress(MemberContactRequest requestModel)
    {
        var checkInsert = await _dialogService.Show<ContactMemberDialog>(_loc["_Member_Update_Address"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }, parameters: new() { { "ContactModel", requestModel }, { "FunctionName", "ContactUpdate" } }).Result;
        var dialogresponseData = checkInsert.Data.CastModel<MemberContactRequest>();

        var contactData = (from i in AddressList where i.MemberContactID == (dialogresponseData.MemberContactID ?? 0) select i).FirstOrDefault();
        if (contactData != null) contactData = dialogresponseData;
    }

    private async Task RemoveAddress(int? memberContactID)
    {
        if (memberContactID is null) return;
        int memberID = await _tools.GetRequesterMemberID();

        var serviceResponse = await _member.ContactDelete(new() { MemberContactID = memberContactID.Value, CurrentMemberID = memberID });
        if (serviceResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Member_Profile_Addresses"]}", Severity.Success);
            AddressList.Remove(AddressList.FirstOrDefault(i => i.MemberContactID == memberContactID));
            StateHasChanged();
            return;
        }
        _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Member_Profile_Addresses"]}", Severity.Error);
    }

}
