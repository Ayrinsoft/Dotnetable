using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_File_Temp_Store
{
    public int FileTempID { get; set; }

    public int FileID { get; set; }

    public DateTime ExpireTime { get; set; }

    public virtual TB_File File { get; set; }
}
