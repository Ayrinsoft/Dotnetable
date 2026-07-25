using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductQuestion
{
    public int ProductQuestionID { get; set; }

    public int WebsiteID { get; set; }

    public int ProductID { get; set; }

    public int WebsiteClientID { get; set; }

    public string Body { get; set; } = null!;

    public byte Status { get; set; } = (byte)1;

    public DateTime CreatedAt { get; set; }

    public bool Approved { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductAnswer> ProductAnswers { get; set; } = new List<ProductAnswer>();

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;
}
