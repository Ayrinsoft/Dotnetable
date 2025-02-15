namespace Dotnetable.Shared.DTO.Post;

public class PostCategoryUpdateOtherLanguageRequest
{
    public int PostCategoryID { get; set; }
    public string LanguageCode { get; set; }
    public string Title { get; set; }
    public string Tags { get; set; }
    public string MetaKeywords { get; set; }
    public string MetaDescription { get; set; }
    public string Description { get; set; }

}
