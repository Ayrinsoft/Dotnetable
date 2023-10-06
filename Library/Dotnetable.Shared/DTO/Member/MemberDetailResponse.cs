using Dotnetable.Shared.DTO.Place;

namespace Dotnetable.Shared.DTO.Member;

public class MemberDetailResponse : CityFormDataResponse
{
    public string Email { get; set; }
    public string CellphoneNumber { get; set; }
    public DateTime RegisterDate { get; set; }
    public string Givenname { get; set; }
    public string Surname { get; set; }
    public int MemberID { get; set; }
    public string CountryCode { get; set; }
    public string Username { get; set; }
    public int CityID { get; set; }
    public bool? Gender { get; set; }
    public Guid? AvatarID { get; set; }
    public string PostalCode { get; set; }
    public List<MemberContactRequest> Addresses { get; set; }

    public Public.ErrorExceptionResponse ErrorException { get; set; }
}
