namespace Dotnetable.Shared.DTO.Post;

public class PostPublicUpdateRequest
{
    public string FinalPostBody { get; set; }
    public string PostCode { get; set; }
    public PostPublicPageDetailUpdateRequest PublicPostDetail { get; set; }
}
