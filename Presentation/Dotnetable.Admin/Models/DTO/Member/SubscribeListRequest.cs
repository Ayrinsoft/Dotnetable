using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Admin.Models.DTO.Member;

public class SubscribeListRequest: GridviewRequest
{
    public string Email { get; set; }
    public int? MemberID { get; set; }
    public bool? Active { get; set; }
}
