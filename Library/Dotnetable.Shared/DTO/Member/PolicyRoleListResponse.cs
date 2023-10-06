namespace Dotnetable.Shared.DTO.Member;

public class PolicyRoleListResponse
{
    public int DatabaseRecords { get; set; }
    public List<RoleDetail> Roles { get; set; }
    public Public.ErrorExceptionResponse ErrorException { get; set; }

    public class RoleDetail
    {
        public int PolicyRoleID { get; set; }
        public short RoleID { get; set; }
        public string RoleKey { get; set; }
    }
}
