using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Member;

public class MemberAvatarInsertRequest
{
    public int? CurrentMemberID { get; set; }
    public int? UploaderMemberID { get; set; }
    public int? FileID { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_File_FileName_Required))]
    [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_36))]
    [MinLength(4, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_4))]
    public string FileName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_File_FileStream_Required))]
    public byte[] FileStream { get; set; }
    public bool SetAsDefault { get; set; }
}
