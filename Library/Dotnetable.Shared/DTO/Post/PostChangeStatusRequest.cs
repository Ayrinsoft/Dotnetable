using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Post;

public class PostChangeStatusRequest
{
    [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Post_PostID_Required))]
    [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Public_Enter_Valid_Integer))]
    public int PostID { get; set; }
}
