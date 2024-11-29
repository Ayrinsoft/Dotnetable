using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Post;

public class SlideShowUpdateRequest
{
    public int CurrentMemberID { get; set; }
    [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_SlideShowID_Required)), Range(1, byte.MaxValue, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Public_Enter_Valid_Integer))]
    public int SlideShowID { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_FileCode_Required))]
    [StringLength(36, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_36))]
    [MinLength(36, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_36))]
    public string FileCode { get; set; }
    
    public string Title { get; set; }
    public string SettingsArray { get; set; }
    public byte? Priority { get; set; }
    public string PageCode { get; set; }
}
