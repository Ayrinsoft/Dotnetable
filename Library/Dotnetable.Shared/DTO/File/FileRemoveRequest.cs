namespace Dotnetable.Shared.DTO.File;

public class FileRemoveRequest
{
    public int CurrentMemberID { get; set; }
    public byte FileCategoryID { get; set; }
    public string FilePath { get; set; }
    public string FileCode { get; set; }
}
