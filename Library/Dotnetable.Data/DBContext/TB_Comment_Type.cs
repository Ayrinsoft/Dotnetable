using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Comment_Type
{
    public byte CommentTypeID { get; set; }

    public string Title { get; set; }

    public virtual ICollection<TB_Post_Comment> TB_Post_Comments { get; set; } = new List<TB_Post_Comment>();
}
