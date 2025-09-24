namespace Dotnetable.Admin.Models.Charts.DTO.Authentication;

public class JWTGenerateRequest
{
    public string HashKey { get; set; }
    public string LogHashKey { get; set; }
    public int MemberID { get; set; }
    public string TokenHashKey { get; set; }
}
