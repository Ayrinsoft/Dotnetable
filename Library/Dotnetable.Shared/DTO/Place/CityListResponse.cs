namespace Dotnetable.Shared.DTO.Place;

public class CityListResponse
{
    public List<CityDetail> Cities { get; set; }
    public Public.ErrorExceptionResponse ErrorException { get; set; }

    public class CityDetail
    {
        public int CityID { get; set; }
        public byte CountryID { get; set; }
        public string Title { get; set; }
        public string LanguageCode { get; set; }
    }
}
