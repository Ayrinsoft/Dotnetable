using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.File;

public class FileInsertRequest
{
    public int CurrentMemberID { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_File_FileName_Required))]
    [StringLength(36, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_36))]
    [MinLength(4, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_4))]
    public string FileName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_File_FileStream_Required))]
    public byte[] FileStream { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_File_FileCategoryID_Required))]
    public byte FileCategoryID { get; set; }

    public string FilePath { get; set; }
    public string FileCode { get; set; }
    public int? UploaderID { get; set; }
}
