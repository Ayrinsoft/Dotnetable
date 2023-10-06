using Dotnetable.Shared.DTO.Place;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Dotnetable.Admin.Components.Member.Profile.Contact
{
    public partial class ContactForm
    {
        [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
        [Parameter] public Dotnetable.Shared.DTO.Member.MemberContactRequest FormModel { get; set; }
        [Parameter] public EventCallback<Dotnetable.Shared.DTO.Member.MemberContactRequest> OnSubmitObject { get; set; }
        private byte _selectedCountryID = 0;

        protected override void OnInitialized()
        {
            FormModel ??= new();            
        }

        private async Task OnSubmitForm()
        {
            await OnSubmitObject.InvokeAsync(FormModel);
        }

        private void BindCities(CountryDetailResponse selectedCountry)
        {
            _selectedCountryID = selectedCountry.CountryID;
        }

        private void UpdateCityItem(CityDetailResponse selectedCity)
        {
            FormModel.CityID = selectedCity.CityID;
            FormModel.CityName = selectedCity.Title;
        }

    }
}
