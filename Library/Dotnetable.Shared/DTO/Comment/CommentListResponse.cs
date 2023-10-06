using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Comment;

public class CommentListResponse
{
    public int DatabaseRecords { get; set; }
    public List<CommentDetail> Comments { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class CommentDetail
    {
        public int ItemID { get; set; }
        public int? ReplyID { get; set; }
        public DateTime LogTime { get; set; }
        public string CommentBody { get; set; }
        public string LanguageCode { get; set; }
        public bool Gender { get; set; }
        public string Surname { get; set; }
    }
}
