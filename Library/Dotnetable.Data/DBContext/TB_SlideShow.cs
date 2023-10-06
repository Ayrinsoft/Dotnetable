using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_SlideShow
{
    public int SlideShowID { get; set; }

    public Guid FileCode { get; set; }

    public string Title { get; set; }

    public string SettingArray { get; set; }

    public bool Active { get; set; }

    public byte Priority { get; set; }

    public string PageCode { get; set; }

    public string LanguageCode { get; set; }

    public virtual ICollection<TB_SlideShow_Language> TB_SlideShow_Languages { get; set; } = new List<TB_SlideShow_Language>();
}
