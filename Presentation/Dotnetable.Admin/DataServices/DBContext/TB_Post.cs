using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Post
{
    public int PostID { get; set; }

    public string Title { get; set; }

    public string Summary { get; set; }

    public string Body { get; set; }

    public int PostCategoryID { get; set; }

    public string Tags { get; set; }

    public string MetaKeywords { get; set; }

    public string MetaDescription { get; set; }

    public int MemberID { get; set; }

    public DateTime LogTime { get; set; }

    public bool Active { get; set; }

    public Guid? FileCode { get; set; }

    public string PostCode { get; set; }

    public bool NormalBody { get; set; }

    public int VisitCount { get; set; }

    public string LanguageCode { get; set; }

    public virtual TB_Member Member { get; set; }

    public virtual TB_Post_Category PostCategory { get; set; }

    public virtual ICollection<TBM_Post_Category_Relation> TBM_Post_Category_Relations { get; set; } = new List<TBM_Post_Category_Relation>();

    public virtual ICollection<TBM_Post_File> TBM_Post_Files { get; set; } = new List<TBM_Post_File>();

    public virtual ICollection<TB_Post_Comment> TB_Post_Comments { get; set; } = new List<TB_Post_Comment>();

    public virtual ICollection<TB_Post_Language> TB_Post_Languages { get; set; } = new List<TB_Post_Language>();
}
