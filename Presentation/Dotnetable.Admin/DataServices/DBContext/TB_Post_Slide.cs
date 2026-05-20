using System;
using System.Collections.Generic;

namespace Dotnetable.Admin.DataServices.DBContext;

public partial class TB_Post_Slide
{
    public int PostSlideID { get; set; }

    public int PostID { get; set; }

    public string Slug { get; set; }

    public Guid FileCode { get; set; }

    public string Description { get; set; }

    public virtual TB_Post Post { get; set; }
}
