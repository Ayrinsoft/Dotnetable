namespace Dotnetable.Admin.Models.DTO.Post;

public class ContactUsUpdateRequest
{
    public PostPublicPageDetailUpdateRequest PublicPostDetail { get; set; }
    public StaticPageDetailContactUsResponse ContactUsDetail { get; set; }
}
