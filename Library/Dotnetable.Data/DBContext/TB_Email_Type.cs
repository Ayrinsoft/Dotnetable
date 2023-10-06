using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Email_Type
{
    public byte EmailTypeID { get; set; }

    public string Title { get; set; }

    public virtual ICollection<TB_Email_Setting> TB_Email_Settings { get; set; } = new List<TB_Email_Setting>();
}
