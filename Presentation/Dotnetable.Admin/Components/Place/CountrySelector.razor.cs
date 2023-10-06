using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.DTO.Place;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Dotnetable.Admin.Components.Place;

public partial class CountrySelector
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IMemoryCache _mmc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }

    [Parameter] public EventCallback<CountryDetailResponse> OnChangeCountryEvent { get; set; }
    [Parameter] public byte? PassedCountryID { get; set; }

    private CountryDetailResponse _selectedCountryItem { get; set; } = null;
    private List<CountryDetailResponse> _countryList { get; set; }
    private string _languageCode;


    protected override async Task OnInitializedAsync()
    {
        if (!_mmc.TryGetValue("PlaceCountryList", out List<CountryDetailResponse> countryList))
        {
            var fetchServiceData = await _httpService.CallServiceObjAsync(HttpMethod.Get, false, "Place/CountryList");
            if (fetchServiceData.Success)
            {
                var serviceCountries = fetchServiceData.ResponseData.CastModel<CountryListResponse>().Countries;
                countryList = serviceCountries.CastModel<List<CountryDetailResponse>>();
                _mmc.Set("PlaceCountryList", countryList, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5)));
            }
        }
        _countryList = countryList;
        _languageCode = CultureInfo.CurrentUICulture.Name.ToUpper().Split('-')[0];

        if (PassedCountryID.HasValue && _selectedCountryItem is null)
            _selectedCountryItem = _countryList.Where(i => i.CountryID == PassedCountryID.Value && (i.LanguageCode == _languageCode || i.LanguageCode == "EN")).FirstOrDefault();
    }

    private async Task<IEnumerable<CountryDetailResponse>> SearchCountry(string searchKey)
    {
        IEnumerable<CountryDetailResponse> countryResponse = _countryList.Where(i => i.LanguageCode == _languageCode);
        if (countryResponse is null || !countryResponse.Any()) countryResponse = _countryList.Where(i => i.LanguageCode == "EN");

        if (string.IsNullOrEmpty(searchKey)) return await Task.FromResult(countryResponse);
        return await Task.FromResult(countryResponse.Where(i => i.Title.Contains(searchKey, StringComparison.InvariantCultureIgnoreCase)));
    }

    private async Task CountrySelectedChange(CountryDetailResponse selectedItem)
    {
        _selectedCountryItem = selectedItem;
        await OnChangeCountryEvent.InvokeAsync(selectedItem);
    }

}