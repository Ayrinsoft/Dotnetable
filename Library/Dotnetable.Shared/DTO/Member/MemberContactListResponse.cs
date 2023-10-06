using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Member;

public class MemberContactListResponse
{
    public List<ContactDetail> Contacts { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class ContactDetail
    {
        public int MemberContactID { get; set; }
        public int MemberID { get; set; }
        public string Address { get; set; }
        public string CellphoneNumber { get; set; }
        public string HomeOwnerName { get; set; }
        public string PhoneNumber { get; set; }
        public string LanguageCode { get; set; }
        public int CityID { get; set; }
        public string CityName { get; set; }
        public byte CountryID { get; set; }
        public string PointLatitude { get; set; }
        public string PointLongitude { get; set; }
        public string PostalCode { get; set; }
    }
}
