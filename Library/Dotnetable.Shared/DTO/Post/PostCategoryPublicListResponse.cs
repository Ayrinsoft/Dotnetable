using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Post;

public class PostCategoryPublicListResponse
{
    public List<PostCategoryDetail> PostCategories { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class PostCategoryDetail
    {
        public int PostCategoryID { get; set; }
        public int? ParentID { get; set; }
        public string Title { get; set; }
        public short Priority { get; set; }
        public string LanguageCode { get; set; }
        public string Description { get; set; }
    }
}
