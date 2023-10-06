using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Post;

public class PostDetailPublicResponse
{
    public List<PostDetails> PostsDetail { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class PostDetails
    {
        public int PostID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string LanguageCode { get; set; }
        public string Body { get; set; }
        public Guid? FileCode { get; set; }
        public string Tags { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public DateTime LogTime { get; set; }
        public string MemberGivenName { get; set; }
        public string MemberSurname { get; set; }
        public bool NormalBody { get; set; }
        public string PostCategoryTitle { get; set; }
    }
}
