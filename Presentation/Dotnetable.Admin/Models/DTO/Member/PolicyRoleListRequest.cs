using Dotnetable.SharedDTO.p.Public;

namespace Dotnetable.Admin.Models.Charts.DTO.Member;

public class PolicyRoleListRequest: GridviewRequest
{
    public int PolicyID { get; set; }
    public bool? ActiveRelation { get; set; }
}
