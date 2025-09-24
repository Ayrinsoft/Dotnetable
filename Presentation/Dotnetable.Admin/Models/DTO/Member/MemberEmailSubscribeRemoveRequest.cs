namespace Dotnetable.Admin.Models.Charts.DTO.Member;

public class MemberEmailSubscribeRemoveRequest
{
    public string CheckHash { get; set; }
    public string EmailAddress { get; set; }
    public int? CurrentMemberID { get; set; }
}
