using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_IP_Address_Action
{
    public int IPAddressActionID { get; set; }

    public string TryIP { get; set; }

    public DateTime LogTime { get; set; }
}
