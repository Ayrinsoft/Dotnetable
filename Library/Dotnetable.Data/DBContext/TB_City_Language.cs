using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_City_Language
{
    public int CityLanguageID { get; set; }

    public int CityID { get; set; }

    public string LanguageCode { get; set; }

    public string Title { get; set; }

    public virtual TB_City City { get; set; }
}
