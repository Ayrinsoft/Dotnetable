using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Login_Try
{
    public int LoginTryID { get; set; }

    public string Username { get; set; }

    public DateTime LogTime { get; set; }

    public bool IsSuccess { get; set; }

    public string TryIP { get; set; }
}
