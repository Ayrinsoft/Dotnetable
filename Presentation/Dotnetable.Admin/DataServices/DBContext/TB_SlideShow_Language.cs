using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_SlideShow_Language
{
    public int SlideShowLanguageID { get; set; }

    public int SlideShowID { get; set; }

    public string Title { get; set; }

    public string LanguageCode { get; set; }

    public string SettingArray { get; set; }

    public virtual TB_SlideShow SlideShow { get; set; }
}
