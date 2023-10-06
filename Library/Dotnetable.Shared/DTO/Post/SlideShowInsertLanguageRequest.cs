using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Post;

public class SlideShowInsertLanguageRequest
{
    [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_SlideShowID_Required))]
    [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Public_Enter_Valid_Integer))]
    public int SlideShowID { get; set; }
        
    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_LanguageCode_Required))]
    [StringLength(2, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_2))]
    [MinLength(2, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_2))]
    public string LanguageCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Title_Required))]
    [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
    [MinLength(4, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_4))]
    public string Title { get; set; }

    [MaxLength(4000, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_4000))]
    public string SettingsArray { get; set; }
}
