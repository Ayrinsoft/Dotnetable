namespace Dotnetable.Admin.Models.Charts.DTO.Authentication;

public class RefreshTokenRequest
{
    public int MemberID { get; set; }
    public string ClientIP { get; set; }
}
