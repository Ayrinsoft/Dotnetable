namespace Dotnetable.Shared.DTO.Authentication;

public class RefreshTokenResponse
{
    public int MemberID { get; set; }
    public Guid Token { get; set; }
    public DateTime ExpireTime { get; set; }
    public string ClientIP { get; set; }
}
