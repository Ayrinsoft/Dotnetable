using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_State_Language
{
    public int StateLanguageID { get; set; }

    public string Tile { get; set; }

    public string LanguageCode { get; set; }

    public int StateID { get; set; }

    public virtual TB_State State { get; set; }
}
