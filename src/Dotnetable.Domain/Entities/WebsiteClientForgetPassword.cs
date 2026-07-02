using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WebsiteClientForgetPassword
{
    public int WebsiteClientForgetPasswordID { get; set; }

    public string ForgetKey { get; set; } = null!;

    public int WebsiteClientID { get; set; }

    public DateTime LogTime { get; set; }

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;
}
