using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Authentication;

public class CaptchaReadRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Authentication_CaptchaCode_Required))]
    public Guid CaptchaCode { get; set; }
}
