using Dotnetable.SharedDTO.p.Public;

namespace Dotnetable.Admin.Models.Charts.DTO.Post;

public class PostCommentListAdminRequest: GridviewRequest
{
    public int? ReplyID { get; set; }
    public int? PostID { get; set; }
    public byte? CommentTypeID { get; set; }
}
