using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Member;

public class PolicyRoleListRequest: GridviewRequest
{
    public int PolicyID { get; set; }
    public bool? ActiveRelation { get; set; }
}
