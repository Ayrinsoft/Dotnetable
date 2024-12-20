﻿using Dotnetable.Service;
using Dotnetable.Shared.DTO.Place;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Dotnetable.Admin.Components.PageComponents.Place;

public partial class CitySelector
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IMemoryCache _mmc { get; set; }
    [Inject] private PlaceService _place { get; set; }

    [Parameter] public EventCallback<CityDetailResponse> OnChangeCityEvent { get; set; }
    [Parameter] public byte SelectedCountryID { get; set; } = 0;
    [Parameter] public int? PassedCityID { get; set; } = 0;
    [Parameter] public byte? PassedCountryID { get; set; }

    private CityDetailResponse _selectedCity { get; set; } = null;
    private List<CityDetailResponse> _cityList { get; set; }
    private string _languageCode;


    protected override async Task OnInitializedAsync()
    {
        if (!_mmc.TryGetValue("PlaceCityList", out List<CityDetailResponse> cityList))
        {
            var fetchServiceData = await _place.CityList();
            if (fetchServiceData.ErrorException is null)
            {
                cityList = fetchServiceData.Cities.CastModel<List<CityDetailResponse>>();
                _mmc.Set("PlaceCityList", cityList, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5)));
            }
        }
        _cityList = cityList;
        _languageCode = CultureInfo.CurrentUICulture.Name.ToUpper().Split('-')[0];

        if (PassedCountryID.HasValue) SelectedCountryID = PassedCountryID.Value;
        if (PassedCityID.HasValue && _selectedCity is null) _selectedCity = _cityList.Where(i => i.CityID == PassedCityID && (i.LanguageCode == _languageCode || i.LanguageCode == "EN")).FirstOrDefault();
    }

    private async Task<IEnumerable<CityDetailResponse>> SearchCity(string searchKey, CancellationToken token)
    {
        if (SelectedCountryID == 0) return null;
        IEnumerable<CityDetailResponse> cityResponse = _cityList.Where(i => i.CountryID == SelectedCountryID && i.LanguageCode == _languageCode);
        if (cityResponse is null || !cityResponse.Any()) cityResponse = _cityList.Where(i => i.CountryID == SelectedCountryID && i.LanguageCode == "EN");

        if (string.IsNullOrEmpty(searchKey)) return await Task.FromResult(cityResponse);
        return await Task.FromResult(cityResponse.Where(i => i.Title.Contains(searchKey, StringComparison.InvariantCultureIgnoreCase)));
    }

    private async Task CitySelectedChange(CityDetailResponse selectedItem)
    {
        _selectedCity = selectedItem;
        await OnChangeCityEvent.InvokeAsync(selectedItem);
    }
}
