using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class FormResponse
{
    public int FormResponseID { get; set; }

    public int FormID { get; set; }

    public int? WebsiteClientID { get; set; }

    public string SenderIPAddress { get; set; } = null!;

    public DateTime SubmittedAt { get; set; }

    public virtual Form Form { get; set; } = null!;

    public virtual ICollection<FormResponseValue> FormResponseValues { get; set; } = new List<FormResponseValue>();

    public virtual WebsiteClient? WebsiteClient { get; set; }
}
