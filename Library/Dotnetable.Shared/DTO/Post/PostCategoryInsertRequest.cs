using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Post;

public class PostCategoryInsertRequest
{
    public int? ParentID { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Post_MenuView_Required))]
    public bool MenuView { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Post_Title_Required))]
    [StringLength(96, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_96))]
    [MinLength(3, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_3))]
    public string Title { get; set; }

    [StringLength(2000, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_2000))]
    public string Tags { get; set; }

    [StringLength(256, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_256))]
    public string MetaKeywords { get; set; }

    [StringLength(256, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_256))]
    public string MetaDescription { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Description_Required))]
    [StringLength(512, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_512))]
    [MinLength(3, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_3))]
    public string Description { get; set; }

    public bool FooterView { get; set; }
    public int? PostCategoryID { get; set; }
    public short? Priority { get; set; }
}
