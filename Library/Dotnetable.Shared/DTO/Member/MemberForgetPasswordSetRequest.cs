using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Member;

public class MemberForgetPasswordSetRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_ForgetKey_Required))]
    [StringLength(8, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_8))]
    [MinLength(4, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_4))]
    public string ForgetKey { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Authentication_Username_Required))]
    [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
    [MinLength(4, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_4))]
    public string Username { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Authentication_Password_Required)), DataType(DataType.Password)]
    [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
    [MinLength(4, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_8))]
    public string Password { get; set; }

    //[Compare("Password", ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Member_SetCompair_Password))]
    //public string ConfirmPassword { get; set; }
}
