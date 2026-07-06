namespace Dotnetable.Domain.Enums;

/// <summary>Input types a form builder field can take. Choice types (Select/Radio/Checkbox) read
/// their choices from <c>FormFieldOptions</c>; Rating uses MinValue/MaxValue as the scale bounds.</summary>
public enum FormFieldType : byte
{
    Text = 0,
    TextArea = 1,
    Number = 2,
    Email = 3,
    Phone = 4,
    Date = 5,
    Select = 6,
    Radio = 7,
    Checkbox = 8,
    Rating = 9,
    YesNo = 10,
    SectionTitle = 11,
}
