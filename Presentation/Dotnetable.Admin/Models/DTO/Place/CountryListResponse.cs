using Dotnetable.SharedDTO.p.Public;

namespace Dotnetable.Admin.Models.Charts.DTO.Place;

public class CountryListResponse
{
    public List<CountryDetail> Countries { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class CountryDetail
    {
        public byte CountryID { get; set; }
        public string Title { get; set; }
        public string LanguageCode { get; set; }
    }
}
