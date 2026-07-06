using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class FormResponseValue
{
    public int FormResponseValueID { get; set; }

    public int FormResponseID { get; set; }

    public int FormFieldID { get; set; }

    /// <summary>Submitted answer. Checkbox (multi-choice) answers are stored as a JSON string array.</summary>
    public string? Value { get; set; }

    public virtual FormResponse FormResponse { get; set; } = null!;

    public virtual FormField FormField { get; set; } = null!;
}
