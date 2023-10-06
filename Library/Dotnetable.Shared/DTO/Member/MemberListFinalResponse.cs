using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Member;

public class MemberListFinalResponse
{
    public int DatabaseRecords { get; set; }
    public List<MemberDetail> Members { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class MemberDetail
    {
        public int MemberID { get; set; }
        public bool Activate { get; set; }
        public bool Active { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string CountryCode { get; set; }
        public DateTime RegisterDate { get; set; }
        public string CellphoneNumber { get; set; }
        public string Givenname { get; set; }
        public string Surname { get; set; }
        public int? CityID { get; set; }
        public byte? CountryID { get; set; }
        public string CityName { get; set; }
        public bool? Gender { get; set; }
        public string PostalCode { get; set; }
        public int PolicyID { get; set; }
    }
}
