using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Language
{
    public byte LanguageID { get; set; }

    public string Title { get; set; }

    public string LocalizedTitle { get; set; }

    public string LanguageCode { get; set; }

    public string LanguageISOCode { get; set; }

    public bool RTLDesign { get; set; }

    public byte? ItemPriority { get; set; }
}
