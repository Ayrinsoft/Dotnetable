namespace Dotnetable.Shared.DTO.Post;

public class PostFileRemoveRequest
{
    public int PostID { get; set; }
    public string FileCode { get; set; }
    public bool CoverImage { get; set; }
}
