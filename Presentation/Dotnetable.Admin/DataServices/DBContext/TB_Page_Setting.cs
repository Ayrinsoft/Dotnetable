using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Page_Setting
{
    public int PageSettingsID { get; set; }

    public byte PagePositionID { get; set; }

    public string ItemTag { get; set; }

    public byte ItemTypeID { get; set; }

    public string ItemBody { get; set; }
}
