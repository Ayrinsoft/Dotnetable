namespace Dotnetable.Admin.Models.Charts.DTO.Member;

public class MemberAvatarDeleteRequest
{
    public int? CurrentMemberID { get; set; }
    public Guid AvatarCode { get; set; }
}
