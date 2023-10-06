namespace Dotnetable.Shared.DTO.Member;

public class MemberAuthorizeResponse
{
    public Public.ErrorExceptionResponse ErrorException { get; set; }
    public bool IsValidAuthorize { get; set; }
    public bool PublicUserPolicy { get; set; }
    public List<PolicyDetail> Policies { get; set; }
    public MemberDetailModel MemberDetail { get; set; }

    public class MemberDetailModel
    {
        public string Email { get; set; }
        public string CellphoneNumber { get; set; }
        public string GivennameLocalized { get; set; }
        public string SurnameLocalized { get; set; }
        public string LocalizedLanguageCode { get; set; }
        public DateTime RegisterDate { get; set; }
        public string Givenname { get; set; }
        public string Surname { get; set; }
        public int MemberID { get; set; }
    }

    public class PolicyDetail
    {
        public string PolicyName { get; set; }
    }
}
