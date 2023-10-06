using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Member;

public class MemberAuthorizeRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Authentication_Username_Required))]
    [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
    [MinLength(4, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_4))]
    public string Username { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Authentication_Password_Required))]
    [StringLength(256, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_256))]
    [MinLength(4, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_8))]
    public string Password { get; set; }

    public bool AdministratorAuthorize { get; set; }
    public string TokenKey { get; set; }
    public byte LanguageID { get; set; }
}
