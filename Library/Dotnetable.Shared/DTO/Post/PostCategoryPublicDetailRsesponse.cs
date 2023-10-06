using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Post;

public class PostCategoryPublicDetailRsesponse
{
    public List<PostCategoryDetail> PostCategories { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class PostCategoryDetail
    {
        public int PostCategoryID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string LanguageCode { get; set; }
    }
}
