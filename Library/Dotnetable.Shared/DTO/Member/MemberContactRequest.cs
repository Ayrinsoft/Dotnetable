using Dotnetable.Shared.DTO.Place;

namespace Dotnetable.Shared.DTO.Member;

public class MemberContactRequest: CityFormDataResponse
{
    public int? MemberContactID { get; set; }
    public int? CurrentMemberID { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string CellphoneNumber { get; set; }
    public string HomeOwnerName { get; set; }
    public string PointLatitude { get; set; }
    public string PointLongitude { get; set; }
    public string LanguageCode { get; set; }
    public bool DefaultContact { get; set; }
    public string PostalCode { get; set; }
    public int CityID { get; set; }
}
