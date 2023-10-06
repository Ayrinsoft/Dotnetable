using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Country
{
    public byte CountryID { get; set; }

    public string CountryCode { get; set; }

    public string LanguageCode { get; set; }

    public string Title { get; set; }

    public string PhonePerfix { get; set; }

    public virtual ICollection<TB_City> TB_Cities { get; set; } = new List<TB_City>();

    public virtual ICollection<TB_Country_Language> TB_Country_Languages { get; set; } = new List<TB_Country_Language>();

    public virtual ICollection<TB_State> TB_States { get; set; } = new List<TB_State>();
}
