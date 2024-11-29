using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Post;

public class SlideShowLanguageListRequest
{
    public int CurrentMemberID { get; set; }
    [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_SlideShowID_Required)), Range(1, byte.MaxValue, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Public_Enter_Valid_Integer))]
    public int SlideShowID { get; set; }
}
