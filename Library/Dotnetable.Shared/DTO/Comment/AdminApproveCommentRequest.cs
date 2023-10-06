using Dotnetable.Shared.DTO.Public;
using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Comment;

public class AdminApproveCommentRequest
{
    [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_CommentCategoryID_Required))]
    [EnumDataType(typeof(CommentCategory))]
    public CommentCategory CommentCategoryID { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Approve_Required))]
    public bool Approve { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_ItemID_Required))]
    [Range(1, 500, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Public_Enter_Valid_Integer))]
    public int ItemID { get; set; }
}
