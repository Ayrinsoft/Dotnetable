using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_State
{
    public int StateID { get; set; }

    public byte CountryID { get; set; }

    public string Tile { get; set; }

    public string LanguageCode { get; set; }

    public bool Active { get; set; }

    public virtual TB_Country Country { get; set; }

    public virtual ICollection<TB_City> TB_Cities { get; set; } = new List<TB_City>();

    public virtual ICollection<TB_State_Language> TB_State_Languages { get; set; } = new List<TB_State_Language>();
}
