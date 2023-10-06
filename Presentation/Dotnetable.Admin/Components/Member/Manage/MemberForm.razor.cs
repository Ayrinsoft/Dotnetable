using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.DTO.Place;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Dotnetable.Admin.Components.Member.Manage;

public partial class MemberForm
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }

    [Parameter] public EventCallback<MemberInsertRequest> OnSubmitObject { get; set; }
    [Parameter] public MemberInsertRequest FormModel { get; set; }
    [Parameter] public string FunctionName { get; set; }

    private byte _selectedCountryID = 0;
    private string _confirmPassword = "";

    protected override void OnInitialized()
    {
        if (FormModel is not null && !string.IsNullOrEmpty(FormModel.Username)) FormModel.Password = _confirmPassword = "QWEqwe@123";

        FormModel ??= new MemberInsertRequest();
    }

    private void BindCities(CountryDetailResponse selectedCountry)
    {
        _selectedCountryID = selectedCountry.CountryID;
    }

    private void UpdateSelectedPolicy(PolicyListOnInsertMemberResponse selectedPolicy)
    {
        FormModel.PolicyID = selectedPolicy.PolicyID;
    }

    private void UpdateCityItem(CityDetailResponse selectedCity)
    {
        FormModel.CityID = selectedCity.CityID;
        FormModel.CityName = selectedCity.Title;
    }

    private async Task OnSubmitForm()
    {
        if (FormModel.Password != _confirmPassword) return;
        await OnSubmitObject.InvokeAsync(FormModel);
    }



}
