using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Member;

public class MemberListRequest: GridviewRequest
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string CellphoneNumber { get; set; }
    public string Givenname { get; set; }
    public string Surname { get; set; }
}
