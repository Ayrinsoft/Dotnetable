namespace Dotnetable.Shared.DTO.Post;

public class ContactUsUpdateRequest
{
    public int CurrentMemberID { get; set; }
    public PostPublicPageDetailUpdateRequest PublicPostDetail { get; set; }
    public StaticPageDetailContactUsResponse ContactUsDetail { get; set; }
}
