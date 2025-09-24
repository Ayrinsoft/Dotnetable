using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TBM_Post_File
{
    public int PostFileID { get; set; }

    public int PostID { get; set; }

    public int FileID { get; set; }

    public bool ShowGallery { get; set; }

    public virtual TB_File File { get; set; }

    public virtual TB_Post Post { get; set; }
}
