using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_City
{
    public int CityID { get; set; }

    public byte CountryID { get; set; }

    public int? StateID { get; set; }

    public string Title { get; set; }

    public string LanguageCode { get; set; }

    public bool Active { get; set; }

    public virtual TB_Country Country { get; set; }

    public virtual TB_State State { get; set; }

    public virtual ICollection<TB_City_Language> TB_City_Languages { get; set; } = new List<TB_City_Language>();

    public virtual ICollection<TB_Member_Contact> TB_Member_Contacts { get; set; } = new List<TB_Member_Contact>();

    public virtual ICollection<TB_Member> TB_Members { get; set; } = new List<TB_Member>();
}
