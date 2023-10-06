namespace Dotnetable.Shared.DTO.Member;

public class MemberEmailSubscribeRegisterRequest
{
    public int? CurrentMemberID { get; set; }
    public string RequestURL { get; set; }
    public string EmailAddress { get; set; }
}
