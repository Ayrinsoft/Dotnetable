namespace Dotnetable.Shared.DTO.Post;

public class PostLanguageDeleteRequest
{
    public int CurrentMemberID { get; set; }
    public int PostID { get; set; }
    public string LanguageCode { get; set; }
}
