using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Member;

public class MemberAvatarListRequest : GridviewRequest
{
    public int? CurrentMemberID { get; set; }
}
