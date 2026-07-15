using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class FormResponseValue
{
    public int FormResponseValueID { get; set; }

    public int FormResponseID { get; set; }

    public int FormFieldID { get; set; }

    public string? Value { get; set; }

    public virtual FormField FormField { get; set; } = null!;

    public virtual FormResponse FormResponse { get; set; } = null!;
}
