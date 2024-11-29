using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.DTO.Place;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Member.Profile;

public partial class ProfileMemberEdit
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private MemberService _member { get; set; }
    [Inject] private Tools _tools { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }


    [Parameter] public MemberEditRequest MemberEdit { get; set; }
    [Parameter] public string FunctionName { get; set; }
    [Parameter] public EventCallback<MemberEditRequest> OnEditMember { get; set; }

    private byte _selectedCountryID = 0;

    private async Task DoUpdateProfile()
    {
        int memberID = await _tools.GetRequesterMemberID();
        PublicActionResponse serviceResponse;
        MemberEdit.CurrentMemberID = memberID;

        if (FunctionName != "EditAdmin")
            serviceResponse = await _member.Register(MemberEdit.CastModel<MemberInsertRequest>());
        else
            serviceResponse = await _member.Edit(MemberEdit);

        if (serviceResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Member_Profile_Detail"]}", Severity.Success);
            await OnEditMember.InvokeAsync(MemberEdit);
            StateHasChanged();
            return;
        }
        _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Member_Profile_Detail"]}", Severity.Error);
    }

    private void BindCities(CountryDetailResponse selectedCountry)
    {
        _selectedCountryID = selectedCountry.CountryID;
    }

    private void UpdateCityItem(CityDetailResponse selectedCity)
    {
        MemberEdit.CityID = selectedCity.CityID;
        MemberEdit.CityName = selectedCity.Title;
    }

}
