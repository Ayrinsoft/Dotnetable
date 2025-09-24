using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_File_Category
{
    public byte FileCategoryID { get; set; }

    public string Tite { get; set; }

    public virtual ICollection<TB_File> TB_Files { get; set; } = new List<TB_File>();
}
