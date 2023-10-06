using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TBM_Post_Category_Relation
{
    public int PostCategoryRelationID { get; set; }

    public int PostID { get; set; }

    public int PostCategoryID { get; set; }

    public bool ShowInList { get; set; }

    public virtual TB_Post Post { get; set; }

    public virtual TB_Post_Category PostCategory { get; set; }
}
