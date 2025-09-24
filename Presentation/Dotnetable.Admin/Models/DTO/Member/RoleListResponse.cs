using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Admin.Models.Charts.DTO.Member;

public class RoleListResponse
{
    public List<RoleDetail> Roles { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class RoleDetail
    {
        public short RoleID { get; set; }
        public string RoleKey { get; set; }
        public string Description { get; set; }
    }
}
