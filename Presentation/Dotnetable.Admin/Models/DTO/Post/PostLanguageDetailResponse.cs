using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Admin.Models.Charts.DTO.Post;

public class PostLanguageDetailResponse
{
    public int PostLanguageID { get; set; }
    public int PostID { get; set; }
    public string Body { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Tags { get; set; }
    public string MetaKeywords { get; set; }
    public string MetaDescription { get; set; }
    public string LanguageCode { get; set; }

    public ErrorExceptionResponse ErrorException { get; set; }
}
