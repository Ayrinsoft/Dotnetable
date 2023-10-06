using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Member;

public class PolicyListRequest : GridviewRequest
{
    public string Title { get; set; }
    public int? CurrentMemberID { get; set; }
}
