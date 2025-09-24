using Dotnetable.Admin.Models.Charts.DTO.Place;
using Dotnetable.Data.DataAccess;

namespace Dotnetable.Service;
public class PlaceService
{

    //TODO: set memory cache
    public async Task<CountryListResponse> CountryList()
    {
        return await PlaceDataAccess.CountryList();
    }

    public async Task<CityListResponse> CityList()
    {
        return await PlaceDataAccess.CityList();
    }




}