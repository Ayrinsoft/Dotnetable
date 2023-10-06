using Dotnetable.Data.DBContext;
using Dotnetable.Shared.DTO.Place;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Data.DataAccess;
public class PlaceDataAccess
{

    public static async Task<CountryListResponse> CountryList()
    {
        using DotnetableEntity db = new();
        var countries = await (from c in db.TB_Countries select new CountryListResponse.CountryDetail { CountryID = c.CountryID, LanguageCode = c.LanguageCode, Title = c.Title })
                            .Concat(from c in db.TB_Countries join cl in db.TB_Country_Languages on c.CountryID equals cl.CountryID select new CountryListResponse.CountryDetail { CountryID = cl.CountryID, LanguageCode = cl.LanguageCode, Title = cl.Title }).ToListAsync();

        return new() { Countries = countries };
    }

    public static async Task<CityListResponse> CityList()
    {
        using DotnetableEntity db = new();

        var cities = await (from c in db.TB_Cities select new CityListResponse.CityDetail { CityID = c.CityID, CountryID = c.CountryID, LanguageCode = c.LanguageCode, Title = c.Title })
                            .Concat(from c in db.TB_Cities join cl in db.TB_City_Languages on c.CityID equals cl.CityID select new CityListResponse.CityDetail { CityID = c.CityID, CountryID = c.CountryID, LanguageCode = cl.LanguageCode, Title = cl.Title }).ToListAsync();

        return new() { Cities = cities };
    }

}