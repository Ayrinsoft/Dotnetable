using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Post;

public class PostCommentListAdminResponse
{
    public int DatabaseRecords { get; set; }
    public List<CommentDetail> Comments { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class CommentDetail
    {
        public int PostCommentID { get; set; }
        public int? ReplyPostCommentID { get; set; }
        public int PostID { get; set; }
        public DateTime LogTime { get; set; }
        public string CommentBody { get; set; }
        public string LanguageCode { get; set; }
        public byte CommentTypeID { get; set; }
        public string IPAddress { get; set; }
        public bool Gender { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string CellphoneNumber { get; set; }
        public int MemberID { get; set; }
    }
}
