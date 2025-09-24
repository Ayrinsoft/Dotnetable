using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Post_Comment
{
    public int PostCommentID { get; set; }

    public byte CommentTypeID { get; set; }

    public DateTime LogTime { get; set; }

    public bool? Active { get; set; }

    public string CommentBody { get; set; }

    public string LanguageCode { get; set; }

    public int PostID { get; set; }

    public int MemberID { get; set; }

    public string IPAddress { get; set; }

    public int? ReplyPostCommentID { get; set; }

    public virtual TB_Comment_Type CommentType { get; set; }

    public virtual ICollection<TB_Post_Comment> InverseReplyPostComment { get; set; } = new List<TB_Post_Comment>();

    public virtual TB_Member Member { get; set; }

    public virtual TB_Post Post { get; set; }

    public virtual TB_Post_Comment ReplyPostComment { get; set; }
}
