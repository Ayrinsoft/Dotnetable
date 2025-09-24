using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_File_Type
{
    public short FileTypeID { get; set; }

    public string FileTypeName { get; set; }

    public string FileExtention { get; set; }

    public string MIMEType { get; set; }

    public virtual ICollection<TB_File> TB_Files { get; set; } = new List<TB_File>();
}
