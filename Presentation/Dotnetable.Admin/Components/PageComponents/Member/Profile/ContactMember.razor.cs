using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Member.Profile
{
    public partial class ContactMember
    {
        [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
        [Inject] private IHttpServices _httpService { get; set; }
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
            var serviceResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, $"Member/{FunctionName}", AddressEdit.ToJsonString());
            if (serviceResponse.Success)
            {
                var paresedServiceResponse = serviceResponse.ResponseData.CastModel<Dotnetable.Shared.DTO.Public.PublicActionResponse>();
                if (paresedServiceResponse.SuccessAction)
                {
                    _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Member_Profile_Addresses"]}", Severity.Success);
                    AddressEdit.MemberContactID = Convert.ToInt32(paresedServiceResponse.ObjectID);
                    await OnInsertOrUpdateContact.InvokeAsync(AddressEdit);
                    StateHasChanged();
                    return;
                }
                else if (paresedServiceResponse.ErrorException != null && !string.IsNullOrEmpty(paresedServiceResponse.ErrorException.ErrorCode))
                {
                    _snackbar.Add($"{_loc[$"_ERROR_{paresedServiceResponse.ErrorException.ErrorCode}"]} {_loc["_Member_Profile_Addresses"]}", Severity.Error);
                    return;
                }
            }
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Member_Profile_Addresses"]}", Severity.Error);
        }

    }
}
