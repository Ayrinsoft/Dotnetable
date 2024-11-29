using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Post;

public class PostDetailRequest
{
    public int CurrentMemberID { get; set; }
    [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Post_PostID_Required))]
    public int PostID { get; set; }
}
