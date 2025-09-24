using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Admin.Models.Charts.DTO.Member;

public class MemberAvatarListRequest : GridviewRequest
{
    public int? CurrentMemberID { get; set; }
}
