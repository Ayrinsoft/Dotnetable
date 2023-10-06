namespace Dotnetable.Shared.DTO.Post;

public class AboutUsUpdateRequest
{
    public PostPublicPageDetailUpdateRequest PublicPostDetail { get; set; }
    public StaticPageDetailAboutUsResponse AboutusDetail { get; set; }
}
