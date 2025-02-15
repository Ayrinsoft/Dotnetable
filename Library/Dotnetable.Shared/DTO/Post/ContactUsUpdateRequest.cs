namespace Dotnetable.Shared.DTO.Post;

public class ContactUsUpdateRequest
{
    public PostPublicPageDetailUpdateRequest PublicPostDetail { get; set; }
    public StaticPageDetailContactUsResponse ContactUsDetail { get; set; }
}
