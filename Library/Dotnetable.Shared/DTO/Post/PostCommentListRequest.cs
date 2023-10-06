using Dotnetable.Shared.DTO.Public;
using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Post;

public class PostCommentListRequest: GridviewRequest
{
    public int? ReplyID { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Post_PostID_Required)), Range(1, 500, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Public_Enter_Valid_Integer))]
    public int PostID { get; set; }
}
