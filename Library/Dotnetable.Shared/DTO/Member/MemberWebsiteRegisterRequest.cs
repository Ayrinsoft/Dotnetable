using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Member;

public class MemberWebsiteRegisterRequest : MemberInsertRequest
{
    [Compare("Password", ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Member_SetCompare_Password))]
    public string ConfirmPassword { get; set; }
}
