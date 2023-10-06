using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.Tools;

public class NotEmptyAttribute : ValidationAttribute
{
    public override bool IsValid(object value) =>
        value is IEnumerable<object> list && list.Any();
    
}
