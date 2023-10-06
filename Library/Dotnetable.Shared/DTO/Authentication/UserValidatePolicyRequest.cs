using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Authentication;

public class UserValidatePolicyRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Authentication_UserHashKey_Required))]
    public Guid UserHashKey { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Authentication_Token_Required))]
    public List<string> RoleNames { get; set; }
}
