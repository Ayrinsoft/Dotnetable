using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Member.Profile
{
    public partial class ContactMember
    {
        [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
        [Inject] private MemberService _member { get; set; }
        [Inject] private Tools _tools { get; set; }
        [Inject] private ISnackbar _snackbar { get; set; }

        [Parameter] public Dotnetable.Shared.DTO.Member.MemberContactRequest AddressEdit { get; set; }
        [Parameter] public string FunctionName { get; set; }
        [Parameter] public EventCallback<Dotnetable.Shared.DTO.Member.MemberContactRequest> OnInsertOrUpdateContact { get; set; }
        [Parameter] public int? MemberID { get; set; }

        private async Task OnEditContactComplete(Dotnetable.Shared.DTO.Member.MemberContactRequest insertedContact)
        {
            AddressEdit = insertedContact;
            await DoUpdateAddress();
        }


        protected override void OnInitialized()
        {
            if (FunctionName == "ContactInsert")
            {
                AddressEdit = new Dotnetable.Shared.DTO.Member.MemberContactRequest();
            }
        }

        private async Task DoUpdateAddress()
        {
            int memberID = await _tools.GetRequesterMemberID();
            AddressEdit.CurrentMemberID = memberID;

            PublicActionResponse serviceResponse;
            if (FunctionName == "ContactInsert")
                serviceResponse = await _member.ContactInsert(AddressEdit);
            else
                serviceResponse = await _member.ContactUpdate(AddressEdit);

            //.CallServiceObjAsync(HttpMethod.Post, true, $"Member/{FunctionName}", AddressEdit.ToJsonString());
            if (serviceResponse.SuccessAction)
            {
                _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Member_Profile_Addresses"]}", Severity.Success);
                AddressEdit.MemberContactID = Convert.ToInt32(serviceResponse.ObjectID);
                await OnInsertOrUpdateContact.InvokeAsync(AddressEdit);
                StateHasChanged();
                return;
            }
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Member_Profile_Addresses"]}", Severity.Error);
        }

    }
}
