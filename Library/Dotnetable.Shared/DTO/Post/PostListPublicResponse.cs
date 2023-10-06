using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Post;

public class PostListPublicResponse
{
    public List<PostDetail> Posts { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class PostDetail
    {
        public int PostID { get; set; }
        public string Title { get; set; }
        public string LanguageCode { get; set; }
        public string Summary { get; set; }
        public Guid? FileCode { get; set; }
        public DateTime LogTime { get; set; }
    }
}
