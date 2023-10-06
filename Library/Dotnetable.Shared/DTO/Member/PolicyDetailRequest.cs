using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Member;

public class PolicyDetailRequest
{
    [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_PolicyID_Required))]
    [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Public_Enter_Valid_Integer))]
    public int PolicyID { get; set; }
}
