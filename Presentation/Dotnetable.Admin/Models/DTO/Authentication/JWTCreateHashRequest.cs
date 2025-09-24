namespace Dotnetable.Admin.Models.Charts.DTO.Authentication;

public class JWTCreateHashRequest
{
    public string UserHashKey { get; set; }
    public string LogHashKey { get; set; }
    public int MemberID { get; set; }
}
