using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Country_Language
{
    public short CountryLanguageID { get; set; }

    public string LanguageCode { get; set; }

    public string Title { get; set; }

    public byte CountryID { get; set; }

    public virtual TB_Country Country { get; set; }
}
