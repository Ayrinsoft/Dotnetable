using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Setting
{
    public int SettingID { get; set; }

    public string SettingKey { get; set; }

    public string SettingValue { get; set; }
}
