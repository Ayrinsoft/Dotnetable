using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Post_Language
{
    public int PostLanguageID { get; set; }

    public string LanguageCode { get; set; }

    public int PostID { get; set; }

    public string Title { get; set; }

    public string Summary { get; set; }

    public string Body { get; set; }

    public string Tags { get; set; }

    public string MetaKeywords { get; set; }

    public string MetaDescription { get; set; }

    public virtual TB_Post Post { get; set; }
}
