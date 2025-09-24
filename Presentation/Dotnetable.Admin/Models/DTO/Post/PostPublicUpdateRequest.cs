namespace Dotnetable.Admin.Models.Charts.DTO.Post;

public class PostPublicUpdateRequest
{
    public string FinalPostBody { get; set; }
    public string PostCode { get; set; }
    public PostPublicPageDetailUpdateRequest PublicPostDetail { get; set; }
}
