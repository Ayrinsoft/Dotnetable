using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Post;

public class PostCategoryDetailResponse
{
    public bool MenuView { get; set; }
    public string Title { get; set; }
    public string Tags { get; set; }
    public string MetaKeywords { get; set; }
    public string MetaDescription { get; set; }
    public bool FooterView { get; set; }
    public bool Active { get; set; }
    public int PostCategoryID { get; set; }
    public short? Priority { get; set; }
    public string LanguageCode { get; set; }
    public string Description { get; set; }

    public ErrorExceptionResponse ErrorException { get; set; }
}
