using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Post;

public class PostCommentListAdminRequest: GridviewRequest
{
    public int? ReplyID { get; set; }
    public int? PostID { get; set; }
    public byte? CommentTypeID { get; set; }
}
