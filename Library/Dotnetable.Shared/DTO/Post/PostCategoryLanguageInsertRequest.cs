using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Post;

public class PostCategoryLanguageInsertRequest
{
    public int PostCategoryID { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Post_Title_Required))]
    [StringLength(128, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_128))]
    [MinLength(3, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_3))]
    public string Title { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Public_LanguageID_Required))]
    public byte LanguageID { get; set; }

    [StringLength(2000, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_2000))]
    public string Tags { get; set; }

    [StringLength(256, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_256))]
    public string MetaKeywords { get; set; }

    [StringLength(256, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_256))]
    public string MetaDescription { get; set; }
}
