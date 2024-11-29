namespace Dotnetable.Shared.DTO.Member;

public class MemberEmailSubscribeRemoveSendCodeRequest
{
    public int CurrentMemberID { get; set; }
    public string EmailAddress { get; set; }
    public string RequestURL { get; set; }
}
