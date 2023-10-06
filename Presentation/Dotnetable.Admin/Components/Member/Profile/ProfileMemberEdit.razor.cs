using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.DTO.Place;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.Member.Profile;

public partial class ProfileMemberEdit
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }    


    [Parameter] public MemberEditRequest MemberEdit { get; set; }
    [Parameter] public string FunctionName { get; set; }
    [Parameter] public EventCallback<MemberEditRequest> OnEditMember { get; set; }

    private byte _selectedCountryID = 0;

    private async Task DoUpdateProfile()
    {
        var updateProfile = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, $"Member/{FunctionName}", MemberEdit.ToJsonString());
        if (updateProfile.Success)
        {
            var parsedUpdateProfile = updateProfile.ResponseData.CastModel<Dotnetable.Shared.DTO.Public.PublicActionResponse>();
            if (parsedUpdateProfile.SuccessAction)
            {
                _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Member_Profile_Detail"]}", Severity.Success);
                await OnEditMember.InvokeAsync(MemberEdit);
                StateHasChanged();
                return;
            }
            else if (parsedUpdateProfile.ErrorException != null && !string.IsNullOrEmpty(parsedUpdateProfile.ErrorException.ErrorCode))
            {
                _snackbar.Add($"{_loc[$"_ERROR_{parsedUpdateProfile.ErrorException.ErrorCode}"]} {_loc["_Member_Profile_Detail"]}", Severity.Error);
                return;
            }
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
