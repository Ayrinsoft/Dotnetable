namespace Dotnetable.Admin.Models.Charts.DTO.Authentication;

public class RefreshTokenValidateRequest
{
    public Guid UserHashKey { get; set; }
    public Guid RefreshToken { get; set; }
    public string ClientIP { get; set; }
}
