using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class FormFieldOption
{
    public int FormFieldOptionID { get; set; }

    public int FormFieldID { get; set; }

    public string Label { get; set; } = null!;

    public string? Value { get; set; }

    public int SortOrder { get; set; }

    public virtual FormField FormField { get; set; } = null!;
}
