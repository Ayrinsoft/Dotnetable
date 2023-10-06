using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Post_Category
{
    public int PostCategoryID { get; set; }

    public int? ParentID { get; set; }

    public bool MenuView { get; set; }

    public string Title { get; set; }

    public string Tags { get; set; }

    public string MetaKeywords { get; set; }

    public string MetaDescription { get; set; }

    public short Priority { get; set; }

    public bool FooterView { get; set; }

    public bool Active { get; set; }

    public string Description { get; set; }

    public Guid? FileCode { get; set; }

    public string LanguageCode { get; set; }

    public virtual ICollection<TBM_Post_Category_Relation> TBM_Post_Category_Relations { get; set; } = new List<TBM_Post_Category_Relation>();

    public virtual ICollection<TB_Post_Category_Language> TB_Post_Category_Languages { get; set; } = new List<TB_Post_Category_Language>();

    public virtual ICollection<TB_Post> TB_Posts { get; set; } = new List<TB_Post>();
}
