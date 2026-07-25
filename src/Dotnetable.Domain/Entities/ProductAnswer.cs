using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductAnswer
{
    public int ProductAnswerID { get; set; }

    public int ProductQuestionID { get; set; }

    public int? WebsiteClientID { get; set; }

    public int? VendorID { get; set; }

    public string Body { get; set; } = null!;

    public byte Status { get; set; }

    public int LikeCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool Approved { get; set; }

    public virtual ProductQuestion ProductQuestion { get; set; } = null!;

    public virtual Vendor? Vendor { get; set; }

    public virtual WebsiteClient? WebsiteClient { get; set; }
}
