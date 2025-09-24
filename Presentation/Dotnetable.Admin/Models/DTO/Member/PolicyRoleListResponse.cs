using Dotnetable.SharedDTO.p.Public;

namespace Dotnetable.Admin.Models.Charts.DTO.Member;

public class PolicyRoleListResponse
{
    public int DatabaseRecords { get; set; }
    public List<RoleDetail> Roles { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class RoleDetail
    {
        public int PolicyRoleID { get; set; }
        public short RoleID { get; set; }
        public string RoleKey { get; set; }
    }
}
