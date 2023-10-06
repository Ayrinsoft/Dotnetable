namespace Dotnetable.Shared.DTO.Member;

public class MemberEmailSubscribeApproveRequest
{
    public string CheckHash { get; set; }
    public string EmailAddress { get; set; }
    public int? MemberID { get; set; }
}
