using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class FormField
{
    public int FormFieldID { get; set; }

    public int FormID { get; set; }

    public string Label { get; set; } = null!;

    public byte FieldType { get; set; }

    public string? Placeholder { get; set; }

    public string? HelpText { get; set; }

    public bool IsRequired { get; set; }

    public int? MinValue { get; set; }

    public int? MaxValue { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual Form Form { get; set; } = null!;

    public virtual ICollection<FormFieldOption> FormFieldOptions { get; set; } = new List<FormFieldOption>();

    public virtual ICollection<FormResponseValue> FormResponseValues { get; set; } = new List<FormResponseValue>();
}
