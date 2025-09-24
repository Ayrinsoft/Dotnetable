namespace Dotnetable.Admin.Models.Charts.DTO.Post;

public class PostFileRemoveRequest
{
    public int PostID { get; set; }
    public string FileCode { get; set; }
    public bool CoverImage { get; set; }
}
