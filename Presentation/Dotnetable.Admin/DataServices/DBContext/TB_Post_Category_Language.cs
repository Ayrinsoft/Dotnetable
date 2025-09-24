using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Post_Category_Language
{
    public int PostCategoryLanguageID { get; set; }

    public int PostCategoryID { get; set; }

    public string Title { get; set; }

    public string LanguageCode { get; set; }

    public string Tags { get; set; }

    public string MetaKeywords { get; set; }

    public string MetaDescription { get; set; }

    public string Description { get; set; }

    public virtual TB_Post_Category PostCategory { get; set; }
}
