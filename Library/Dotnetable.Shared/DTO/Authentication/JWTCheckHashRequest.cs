namespace Dotnetable.Shared.DTO.Authentication;

public class JWTCheckHashRequest
{
    public string JWTHashKey { get; set; }
    public string UserHashKey { get; set; }
    public string LogHashKey { get; set; }
    public int MemberID { get; set; }
}
