namespace Dotnetable.Domain.Enums;

/// <summary>What kind of dynamic form this is. Both kinds share the same builder/response engine;
/// surveys additionally support public aggregate results (<c>Form.ShowResults</c>).</summary>
public enum FormType : byte
{
    Form = 0,
    Survey = 1,
}
