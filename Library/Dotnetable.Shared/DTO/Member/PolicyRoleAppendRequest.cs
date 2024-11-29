namespace Dotnetable.Shared.DTO.Member;

public class PolicyRoleAppendRequest
{
    public int PolicyID { get; set; }
    public short? RoleID { get; set; }
    public string RoleKey { get; set; }
    public int CurrentMemberID { get; set; }
}
