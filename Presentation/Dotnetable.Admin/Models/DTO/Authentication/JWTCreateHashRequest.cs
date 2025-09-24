namespace Dotnetable.Admin.Models.DTO.Authentication;

public class JWTCreateHashRequest
{
    public string UserHashKey { get; set; }
    public string LogHashKey { get; set; }
    public int MemberID { get; set; }
}
