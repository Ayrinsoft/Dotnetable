using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Admin.Models.DTO.File;

public class FileUploadImageCKEditorResponse
{
    public string ImageUrl { get; set; }
    public string FileCode { get; set; }
    public bool Success { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }
}
