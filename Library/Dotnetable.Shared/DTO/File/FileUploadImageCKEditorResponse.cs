namespace Dotnetable.Shared.DTO.File;

public class FileUploadImageCKEditorResponse
{
    public string ImageUrl { get; set; }
    public string FileCode { get; set; }
    public bool Success { get; set; }
    public Public.ErrorExceptionResponse ErrorException { get; set; }
}
