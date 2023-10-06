namespace Dotnetable.Shared.DTO.Member;

public class RoleListResponse
{
    public List<RoleDetail> Roles { get; set; }
    public Public.ErrorExceptionResponse ErrorException { get; set; }

    public class RoleDetail
    {
        public short RoleID { get; set; }
        public string RoleKey { get; set; }
        public string Description { get; set; }
    }
}
