namespace Dotnetable.Shared.DTO.File;

public class FileFetchFromDBResponse
{
    public string MIMEType { get; set; }
    public bool Approved { get; set; }
    public byte FileCategoryID { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
}
