using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_File
{
    public int FileID { get; set; }

    public Guid FileCode { get; set; }

    public string FileName { get; set; }

    public short FileTypeID { get; set; }

    public DateTime UploadTime { get; set; }

    public bool Approved { get; set; }

    public string FilePath { get; set; }

    public byte FileCategoryID { get; set; }

    public int UploaderID { get; set; }

    public virtual TB_File_Category FileCategory { get; set; }

    public virtual TB_File_Type FileType { get; set; }

    public virtual ICollection<TBM_Member_File> TBM_Member_Files { get; set; } = new List<TBM_Member_File>();

    public virtual ICollection<TBM_Post_File> TBM_Post_Files { get; set; } = new List<TBM_Post_File>();

    public virtual ICollection<TB_File_Temp_Store> TB_File_Temp_Stores { get; set; } = new List<TB_File_Temp_Store>();

    public virtual TB_Member Uploader { get; set; }
}
