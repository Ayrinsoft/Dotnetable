namespace Dotnetable.Shared.DTO.Post;

public class AboutUsUpdateRequest
{
    public int CurrentMemberID { get; set; }
    public PostPublicPageDetailUpdateRequest PublicPostDetail { get; set; }
    public StaticPageDetailAboutUsResponse AboutusDetail { get; set; }
}
