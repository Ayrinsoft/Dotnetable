namespace Dotnetable.Admin.Models.DTO.Authentication;

public class CapthchaCheckRequest
{
    public Guid CaptchaCode { get; set; }
    public string CaptchaValue { get; set; }
}
