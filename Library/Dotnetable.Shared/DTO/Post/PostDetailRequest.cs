using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Post;

public class PostDetailRequest
{
    [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Post_PostID_Required))]
    public int PostID { get; set; }
}
