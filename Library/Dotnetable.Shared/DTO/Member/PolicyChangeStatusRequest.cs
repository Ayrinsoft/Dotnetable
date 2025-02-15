namespace Dotnetable.Shared.DTO.Member;

public class PolicyChangeStatusRequest
{
    public int PolicyID { get; set; }

    public int? CurrentMemberID { get; set; }
}
