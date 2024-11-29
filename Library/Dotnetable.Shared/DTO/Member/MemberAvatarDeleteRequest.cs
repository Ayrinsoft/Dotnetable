namespace Dotnetable.Shared.DTO.Member;

public class MemberAvatarDeleteRequest
{
    public int? CurrentMemberID { get; set; }
    public int? MemberID { get; set; }
    public Guid AvatarCode { get; set; }
}
