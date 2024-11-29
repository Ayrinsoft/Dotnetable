namespace Dotnetable.Shared.DTO.File;

public class FileMoveFromTMPFolderRequest
{
    public int CurrentMemberID { get; set; }
    public string FileCode { get; set; }
    public string NewFilePath { get; set; }
    public string NewFileCategory { get; set; }
}
