using Dotnetable.SharedDTO.p.Public;

namespace Dotnetable.Admin.Models.Charts.DTO.Place;

public class CityListResponse
{
    public List<CityDetail> Cities { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class CityDetail
    {
        public int CityID { get; set; }
        public byte CountryID { get; set; }
        public string Title { get; set; }
        public string LanguageCode { get; set; }
    }
}
